//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Security.Cryptography; // Needed for hashing
using System.Text;                // Needed for hashing
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging; 
namespace login_store
{
    public partial class ResetPasswordPage : Page
    {
        private SlideManage parent;
        private string userEmail; // To store the email passed from ForgotPasswordPage
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        // Constructor receives the email
        public ResetPasswordPage(SlideManage parent, string email)
        {
            InitializeComponent();
            this.parent = parent;
            this.userEmail = email;
            txtInfo.Text = $"สำหรับอีเมล: {email}"; // Display the email being reset
        }

        // --- Window Control Methods (Copy from other pages) ---
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)

        {

            Window window = Window.GetWindow(this);

            if (window != null)

            {
                window.WindowState = WindowState.Minimized;
            }

        }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)

        {

            Window window = Window.GetWindow(this);

            if (window != null)

            {

                window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized;

            }

        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)

        {

            Application.Current.Shutdown();

        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)

        {

            if (e.OriginalSource is Border)

            {

                Window.GetWindow(this)?.DragMove();

            }

        }
        // --- (เพิ่ม) Password Toggle Button Logic ---
        private void btnToggleNewPassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(txtNewPassword, txtNewPasswordVisible, btnToggleNewPassword, "EyeIconNew");
        }
        private void btnToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(txtConfirmNewPassword, txtConfirmNewPasswordVisible, btnToggleConfirmPassword, "EyeIconConfirm");
        }

        private void TogglePasswordVisibility(PasswordBox pBox, TextBox tBox, System.Windows.Controls.Primitives.ToggleButton toggleBtn, string iconName)
        {
            if (toggleBtn.IsChecked == true)
            {
                tBox.Visibility = Visibility.Visible;
                pBox.Visibility = Visibility.Collapsed;
                tBox.Text = pBox.Password;
                SetEyeIcon(toggleBtn, iconName, "eye-closed.png");
                tBox.Focus();
                tBox.CaretIndex = tBox.Text.Length;
            }
            else
            {
                pBox.Visibility = Visibility.Visible;
                tBox.Visibility = Visibility.Collapsed;
                pBox.Password = tBox.Text;
                SetEyeIcon(toggleBtn, iconName, "eye-open.png");
                pBox.Focus();
            }
        }

        private void SetEyeIcon(System.Windows.Controls.Primitives.ToggleButton toggleBtn, string iconName, string iconFileName)
        {
            try
            {
                if (toggleBtn.Template.FindName(iconName, toggleBtn) is Image eyeImage)
                {
                    Uri iconUri = new Uri($"/Images/{iconFileName}", UriKind.Relative);
                    eyeImage.Source = new BitmapImage(iconUri);
                }
            }
            catch (Exception ex) { Console.WriteLine($"Error loading eye icon: {ex.Message}"); }
        }
        // --- จบส่วน Password Toggle ---


        // --- Save New Password Logic ---
        private void SavePassword_Click(object sender, RoutedEventArgs e)
        {
            // Clear previous errors
            tbPasswordError.Visibility = Visibility.Collapsed;
            tbConfirmPasswordError.Visibility = Visibility.Collapsed;

            string newPassword = txtNewPassword.Password;
            string confirmPassword = txtConfirmNewPassword.Password;
            bool isValid = true;

            // Validate New Password (same criteria as registration)
            if (string.IsNullOrEmpty(newPassword))
            {
                ShowError(tbPasswordError, "กรุณากรอกรหัสผ่านใหม่");
                isValid = false;
            }
            else if (newPassword.Length < 8)
            {
                ShowError(tbPasswordError, "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร");
                isValid = false;
            }
            else if (!Regex.IsMatch(newPassword, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$"))
            {
                ShowError(tbPasswordError, "รหัสผ่านต้องมี (ตัวพิมพ์เล็ก, ตัวพิมพ์ใหญ่ และตัวเลข)");
                isValid = false;
            }

            // Validate Confirmation
            if (newPassword != confirmPassword)
            {
                ShowError(tbConfirmPasswordError, "รหัสผ่านใหม่ไม่ตรงกัน");
                isValid = false;
            }

            if (!isValid) return;

            // --- Update Password in Database ---
            try
            {
                string hashedPassword = HashPassword(newPassword); // Hash the new password

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // Update password_hash and clear OTP fields for the specific email
                    string sql = "UPDATE users SET password_hash = @newHash, otp_code = NULL, otp_expiry = NULL WHERE email = @email";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@newHash", hashedPassword);
                        cmd.Parameters.AddWithValue("@email", this.userEmail); // Use the stored email
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            CustomMessageBoxWindow.Show("ตั้งรหัสผ่านใหม่สำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                            BackToLogin(); // Navigate back to login
                        }
                        else
                        {
                            CustomMessageBoxWindow.Show("เกิดข้อผิดพลาด: ไม่สามารถอัปเดตรหัสผ่านได้", "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึกรหัสผ่านใหม่: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- Enter Key Logic ---
        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtNewPassword || sender == txtNewPasswordVisible)
                {
                    if (txtConfirmNewPassword.Visibility == Visibility.Visible) txtConfirmNewPassword.Focus();
                    else txtConfirmNewPasswordVisible.Focus();
                }
                else if (sender == txtConfirmNewPassword || sender == txtConfirmNewPasswordVisible)
                {
                    SavePassword_Click(btnSavePassword, new RoutedEventArgs());
                }
                e.Handled = true;
            }
        }

        // --- Helper Methods ---
        private string HashPassword(string password) // Identical to the one in RegisterPage
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) { builder.Append(bytes[i].ToString("x2")); }
                return builder.ToString();
            }
        }

        private void ShowError(TextBlock errorLabel, string message)
        {
            errorLabel.Text = message;
            errorLabel.Visibility = Visibility.Visible;
        }

        private void BackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            BackToLogin();
        }

        private void BackToLogin()
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new LoginPage(SlideManage.Instance), true);
            }
        }
    }
}