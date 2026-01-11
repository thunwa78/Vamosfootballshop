using System;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging; // สำหรับ BitmapImage
using System.Linq;
using System.Collections.Generic;

namespace login_store
{
    public partial class RegisterPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        public RegisterPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
        }

        // --- เมธอดควบคุมหน้าต่าง ---
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null)
            {
                window.WindowState = WindowState.Minimized; // 👈 แก้เป็นแบบนี้
            }
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
            if (e.OriginalSource.GetType() == typeof(Border)) // ตรวจสอบว่าคลิกที่ Border โดยตรงหรือไม่
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        // --- เมธอดสำหรับปุ่ม Toggle รหัสผ่าน ---
        // เมธอดเมื่อกดปุ่ม Toggle ข้างช่องรหัสผ่านหลัก
        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(txtPassword, txtPasswordVisible, btnTogglePassword, "EyeIcon");
        }
        // เมธอดเมื่อกดปุ่ม Toggle ข้างช่องยืนยันรหัสผ่าน
        private void btnToggleConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            TogglePasswordVisibility(txtConfirmPassword, txtConfirmPasswordVisible, btnToggleConfirmPassword, "EyeIconConfirm");
        }

        // เมธอดกลางสำหรับสลับการแสดงผลรหัสผ่าน
        private void TogglePasswordVisibility(PasswordBox pBox, TextBox tBox, System.Windows.Controls.Primitives.ToggleButton toggleBtn, string iconName)
        {
            if (toggleBtn.IsChecked == true)
            {
                // แสดงรหัสผ่าน
                tBox.Visibility = Visibility.Visible;     // แสดง TextBox
                pBox.Visibility = Visibility.Collapsed;   // ซ่อน PasswordBox
                tBox.Text = pBox.Password;                // คัดลอกค่าไป TextBox
                SetEyeIcon(toggleBtn, iconName, "eye-closed.png"); // เปลี่ยนเป็นตาปิด
                tBox.Focus();                             // ย้าย Focus
                tBox.CaretIndex = tBox.Text.Length;       // เลื่อน Cursor ไปท้ายสุด
            }
            else
            {
                // ซ่อนรหัสผ่าน
                pBox.Visibility = Visibility.Visible;     // แสดง PasswordBox
                tBox.Visibility = Visibility.Collapsed;   // ซ่อน TextBox
                pBox.Password = tBox.Text;                // คัดลอกค่ากลับไป PasswordBox
                SetEyeIcon(toggleBtn, iconName, "eye-open.png"); // เปลี่ยนเป็นตาเปิด
                pBox.Focus();                             // ย้าย Focus
            }
        }

        // เมธอดช่วยในการเปลี่ยนไอคอนรูปตา
        private void SetEyeIcon(System.Windows.Controls.Primitives.ToggleButton toggleBtn, string iconName, string iconFileName)
        {
            try
            {
                // ค้นหา Control ชื่อ Image (ที่ตั้ง x:Name ไว้) ภายใน Template ของ ToggleButton
                if (toggleBtn.Template.FindName(iconName, toggleBtn) is Image eyeImage)
                {
                    // สร้าง Path ของรูปภาพ
                    Uri iconUri = new Uri($"/Images/{iconFileName}", UriKind.Relative);
                    // กำหนด Source ของ Image ให้เป็นรูปใหม่
                    eyeImage.Source = new BitmapImage(iconUri);
                }
            }
            // จัดการข้อผิดพลาด (เช่น รูปภาพหาไม่เจอ)
            catch (Exception ex) { Console.WriteLine($"เกิดข้อผิดพลาดในการโหลดไอคอนรูปตา: {ex.Message}"); }
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender == txtEmail)
                {
                    // Email -> Username
                    txtUsername.Focus();
                }
                else if (sender == txtUsername)
                {
                    // Username -> Password (ช่องที่แสดง)
                    if (txtPassword.Visibility == Visibility.Visible) txtPassword.Focus();
                    else txtPasswordVisible.Focus();
                }
                else if (sender == txtPassword || sender == txtPasswordVisible)
                {
                    // Password -> Confirm Password (ช่องที่แสดง)
                    if (txtConfirmPassword.Visibility == Visibility.Visible) txtConfirmPassword.Focus();
                    else txtConfirmPasswordVisible.Focus();
                }
                else if (sender == txtConfirmPassword || sender == txtConfirmPasswordVisible)
                {
                    // Confirm Password -> กดปุ่ม Register
                    Register_Click(btnRegister, new RoutedEventArgs());
                }
                e.Handled = true; // หยุด Enter ไม่ให้ทำงานต่อ
            }
        }

        // --- Logic การสมัครสมาชิก ---
        private void Register_Click(object sender, RoutedEventArgs e)
        {
            // --- 1. ซ่อน Error Label เก่า ---
            tbUsernameError.Visibility = Visibility.Collapsed;
            tbEmailError.Visibility = Visibility.Collapsed;
            tbPasswordError.Visibility = Visibility.Collapsed;
            tbConfirmPasswordError.Visibility = Visibility.Collapsed;

            // --- 2. ดึงข้อมูล (อ่านจากช่องที่แสดงผลอยู่) ---
            string email = txtEmail.Text.Trim();
            string username = txtUsername.Text.Trim();
            string password = (txtPassword.Visibility == Visibility.Visible) ? txtPassword.Password : txtPasswordVisible.Text;
            string confirmPassword = (txtConfirmPassword.Visibility == Visibility.Visible) ? txtConfirmPassword.Password : txtConfirmPasswordVisible.Text;
            bool isValid = true;

            // --- 3. ตรวจสอบเงื่อนไข (Validation) ---
            // (ส่วนนี้เหมือนเดิม)
            if (string.IsNullOrEmpty(email)) { ShowError(tbEmailError, "กรุณากรอกอีเมล"); isValid = false; }
            else if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")) { ShowError(tbEmailError, "รูปแบบอีเมลไม่ถูกต้อง"); isValid = false; }

            if (string.IsNullOrEmpty(username)) { ShowError(tbUsernameError, "กรุณากรอกชื่อผู้ใช้"); isValid = false; }
            else if (username.Length < 4) { ShowError(tbUsernameError, "ชื่อผู้ใช้ต้องมีอย่างน้อย 4 ตัวอักษร"); isValid = false; }
            else if (username.Length > 20) { ShowError(tbUsernameError, "ชื่อผู้ใช้ยาวเกิน 20 ตัวอักษร"); isValid = false; }
            else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$")) { ShowError(tbUsernameError, "ชื่อผู้ใช้ห้ามมีอักขระพิเศษหรือภาษาไทย"); isValid = false; }

            if (string.IsNullOrEmpty(password)) { ShowError(tbPasswordError, "กรุณากรอกรหัสผ่าน"); isValid = false; }
            else if (password.Length < 8) { ShowError(tbPasswordError, "รหัสผ่านต้องมีอย่างน้อย 8 ตัวอักษร"); isValid = false; }
            else if (!Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")) { ShowError(tbPasswordError, "รหัสผ่านต้องมี (ตัวพิมพ์เล็ก, ตัวพิมพ์ใหญ่ และตัวเลข)"); isValid = false; }

            if (password != confirmPassword) { ShowError(tbConfirmPasswordError, "รหัสผ่านไม่ตรงกัน"); isValid = false; }

            if (!isValid) return; // ถ้าไม่ผ่านเงื่อนไขเบื้องต้น ให้หยุด

            // --- 4. ตรวจสอบ "ซ้ำ" กับฐานข้อมูล ---
            // (ส่วนนี้เหมือนเดิม)
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // เช็ค Email ซ้ำ
                    string checkEmailSql = "SELECT COUNT(*) FROM users WHERE email = @email";
                    using (MySqlCommand cmdCheckEmail = new MySqlCommand(checkEmailSql, conn))
                    {
                        cmdCheckEmail.Parameters.AddWithValue("@email", email);
                        if (Convert.ToInt32(cmdCheckEmail.ExecuteScalar()) > 0)
                        { ShowError(tbEmailError, "อีเมลนี้ถูกใช้งานแล้ว"); isValid = false; }
                    }
                    // เช็ค Username ซ้ำ
                    string checkUserSql = "SELECT COUNT(*) FROM users WHERE username = @username";
                    using (MySqlCommand cmdCheckUser = new MySqlCommand(checkUserSql, conn))
                    {
                        cmdCheckUser.Parameters.AddWithValue("@username", username);
                        if (Convert.ToInt32(cmdCheckUser.ExecuteScalar()) > 0)
                        { ShowError(tbUsernameError, "ชื่อผู้ใช้นี้ถูกใช้งานแล้ว"); isValid = false; }
                    }
                }
            }
            catch (MySqlException ex) {
                CustomMessageBoxWindow.Show("เชื่อมต่อฐานข้อมูลล้มเหลว: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                isValid = false;
            }

            // --- 5. สรุปผล ---
            if (isValid) // ถ้าผ่านทุกเงื่อนไข
            {
                try
                {
                    // Hash รหัสผ่านที่อ่านมาจากช่องที่แสดงผล
                    string hashedPassword = HashPassword(password);
                    // บันทึกลงฐานข้อมูล
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string insertSql = "INSERT INTO users (email, username, password_hash) VALUES (@email, @username, @password_hash)";
                        using (MySqlCommand cmdInsert = new MySqlCommand(insertSql, conn))
                        {
                            cmdInsert.Parameters.AddWithValue("@email", email);
                            cmdInsert.Parameters.AddWithValue("@username", username);
                            cmdInsert.Parameters.AddWithValue("@password_hash", hashedPassword);
                            cmdInsert.ExecuteNonQuery();
                        }
                    }
                    CustomMessageBoxWindow.Show("สมัครสมาชิกสำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    BackToLogin(); // กลับไปหน้า Login
                }
                catch (Exception ex) { CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึกข้อมูล: " + ex.Message, "Save Error", CustomMessageBoxWindow.MessageBoxType.Error); }
            }
        }

        // เมธอด Hash รหัสผ่าน (SHA256)
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++) { builder.Append(bytes[i].ToString("x2")); }
                return builder.ToString();
            }
        }

        // เมธอดแสดง Error Label
        private void ShowError(TextBlock errorLabel, string message)
        {
            errorLabel.Text = message;
            errorLabel.Visibility = Visibility.Visible;
        }

        // เมธอดเมื่อคลิกลิงก์กลับไปหน้า Login
        private void BackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            BackToLogin();
        }

        // เมธอดช่วยในการกลับไปหน้า Login
        private void BackToLogin()
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new LoginPage(SlideManage.Instance), true);
        }
    }

    // (ส่วน Extension method ไม่จำเป็นต้องมี ถ้าไม่ได้ใช้ในไฟล์อื่น)
    // public static class FrameworkElementExtensions ...
}