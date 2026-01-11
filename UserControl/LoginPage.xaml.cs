//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using login_store.Properties;

namespace login_store
{
    public partial class LoginPage : Page
    {
        private SlideManage parent;

        // (แก้ 1) เปลี่ยน 127.0.0.1 เป็น localhost (ตาม Error Log ของ XAMPP)
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        public LoginPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;


        }

        // --- Window Control Methods ---
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null) { window.WindowState = WindowState.Minimized; }
        }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null) { window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized; }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource.GetType() == typeof(Border))
            {
                Window.GetWindow(this)?.DragMove();
            }
        }
        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (btnTogglePassword.IsChecked == true)
            {
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPassword.Visibility = Visibility.Collapsed;
                txtPasswordVisible.Text = txtPassword.Password;
                SetEyeIcon("eye-closed.png");
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                txtPassword.Visibility = Visibility.Visible;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Password = txtPasswordVisible.Text;
                SetEyeIcon("eye-open.png");
                txtPassword.Focus();
            }
        }
        private void SetEyeIcon(string iconFileName)
        {
            try
            {
                if (btnTogglePassword.Template.FindName("EyeIcon", btnTogglePassword) is Image eyeImage)
                {
                    Uri iconUri = new Uri($"/Images/{iconFileName}", UriKind.Relative);
                    eyeImage.Source = new BitmapImage(iconUri);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading eye icon: {ex.Message}");
            }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtUsername)
                {
                    if (txtPassword.Visibility == Visibility.Visible)
                        txtPassword.Focus();
                    else
                        txtPasswordVisible.Focus();
                }
                else if (sender == txtPassword || sender == txtPasswordVisible)
                {
                    Login_Click(btnLogin, new RoutedEventArgs());
                }
                e.Handled = true;
            }
        }

        // --- Login Logic ---
        private void Login_Click(object sender, RoutedEventArgs e)
        {
            tbUsernameError.Visibility = Visibility.Collapsed;
            tbPasswordError.Visibility = Visibility.Collapsed;

            string input = txtUsername.Text.Trim();

            // (แก้ 2) เพิ่ม .Trim() เพื่อตัดช่องว่างที่ผู้ใช้เผลอพิมพ์
            string password = (txtPassword.Visibility == Visibility.Visible)
                                ? txtPassword.Password.Trim()
                                : txtPasswordVisible.Text.Trim();

            if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(password))
            {
                CustomMessageBoxWindow.Show("กรุณากรอกชื่อผู้ใช้และรหัสผ่าน", "ข้อมูลไม่ครบ", CustomMessageBoxWindow.MessageBoxType.Warning);
                if (string.IsNullOrEmpty(input)) { tbUsernameError.Text = "กรุณากรอกชื่อผู้ใช้"; tbUsernameError.Visibility = Visibility.Visible; }
                if (string.IsNullOrEmpty(password)) { tbPasswordError.Text = "กรุณากรอกรหัสผ่าน"; tbPasswordError.Visibility = Visibility.Visible; }
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT id, username, password_hash, role FROM users WHERE username = @input OR email = @input";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@input", input);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader.GetString("password_hash");
                                string inputHash = HashPassword(password); // <-- รหัสผ่านที่ Trim แล้ว

                                if (storedHash == inputHash)
                                {
                                    int userId = reader.GetInt32("id");
                                    string dbUsername = reader.GetString("username");
                                    string userRole = reader.GetString("role");

                                    UserSession.StartSession(userId, dbUsername, userRole);

                                    CustomMessageBoxWindow.Show("เข้าสู่ระบบสำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);

                                    if (userRole == "admin")
                                    {
                                        if (SlideManage.Instance != null)
                                            SlideManage.Instance.NavigateWithSlide(new AdminDashboardPage(this.parent), false);
                                    }
                                    else
                                    {
                                        if (SlideManage.Instance != null)
                                            SlideManage.Instance.NavigateWithSlide(new ShopPage(this.parent), false);
                                    }
                                }
                                else { ShowLoginError(); } // <-- รหัสผ่านผิด
                            }
                            else { ShowLoginError(); } // <-- ไม่พบ Username/Email
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // (แก้ 3) แสดง Error ที่แท้จริง (อาจจะเป็น "Reading from stream has failed" ถ้า MySQL ล่ม)
                CustomMessageBoxWindow.Show("เชื่อมต่อฐานข้อมูลล้มเหลว: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void ShowLoginError()
        {
            CustomMessageBoxWindow.Show("username และ password ไม่ถูกต้อง", "Login Failed", CustomMessageBoxWindow.MessageBoxType.Error);
            tbUsernameError.Text = "ชื่อผู้ใช้งานไม่ถูกต้อง";
            tbUsernameError.Visibility = Visibility.Visible;
            tbPasswordError.Text = "รหัสผ่านไม่ถูกต้อง";
            tbPasswordError.Visibility = Visibility.Visible;
        }

        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                // โค้ดนี้ไม่ได้แปลงเป็นตัวพิมพ์เล็ก!
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) { builder.Append(bytes[i].ToString("x2")); }
                return builder.ToString();
            }
        }

        // --- Navigation ---
        private void OpenForgotPassword_Click(object sender, MouseButtonEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ForgotPasswordPage(this.parent), false);
        }
        private void OpenRegister_Click(object sender, MouseButtonEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new RegisterPage(this.parent), false);
        }
    }
}