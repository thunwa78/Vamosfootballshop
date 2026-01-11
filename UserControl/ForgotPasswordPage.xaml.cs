using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Windows.Threading; // For Timer
using System.Linq;             // For OfType, Where
using System.Collections.Generic; // For IEnumerable
using MailKit.Net.Smtp;         // For MailKit
using MimeKit;                 // For MailKit
using MailKit.Security;        // For MailKit SecureSocketOptions

namespace login_store
{
    public partial class ForgotPasswordPage : Page
    {
        private SlideManage parent;
        private string _foundEmail; // Stores the email found in the database
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        private DispatcherTimer countdownTimer; // Timer variable
        private int remainingSeconds;         // Countdown variable

        public ForgotPasswordPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;

            // Initialize Timer
            countdownTimer = new DispatcherTimer();
            countdownTimer.Interval = TimeSpan.FromSeconds(1);
            countdownTimer.Tick += CountdownTimer_Tick;
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

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                // ตรวจสอบว่ามาจากช่องไหน
                if (sender == txtUsernameOrEmail)
                {
                    // ถ้ากด Enter ที่ช่อง Email/User, ให้กดปุ่ม Send OTP
                    SendOTP_Click(btnSendOtp, new RoutedEventArgs());
                }
                else if (sender == txtOTP)
                {
                    // ถ้ากด Enter ที่ช่อง OTP, ให้กดปุ่ม Verify OTP
                    VerifyOTP_Click(btnVerifyOtp, new RoutedEventArgs());
                }
                e.Handled = true; // หยุด Enter ไม่ให้ทำงานต่อ
            }
        }


        // --- Forgot Password Logic ---

        // Step 1: User enters username/email and clicks "SEND OTP"
        private void SendOTP_Click(object sender, RoutedEventArgs e)
        {
            tbInputError.Visibility = Visibility.Collapsed;
            string input = txtUsernameOrEmail.Text.Trim();

            if (string.IsNullOrEmpty(input))
            {
                ShowError(tbInputError, "กรุณากรอกชื่อผู้ใช้หรืออีเมล");
                return;
            }

            // Find Email in Database
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT email FROM users WHERE username = @input OR email = @input LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@input", input);
                        object result = cmd.ExecuteScalar();
                        _foundEmail = (result != null && result != DBNull.Value) ? result.ToString() : null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("เชื่อมต่อฐานข้อมูลล้มเหลว: " + ex.Message);
                return;
            }

            if (string.IsNullOrEmpty(_foundEmail))
            {
                ShowError(tbInputError, "ไม่พบชื่อผู้ใช้หรืออีเมลนี้ในระบบ");
                return;
            }

            // Generate/Store OTP and Send Email
            string generatedOtp = GenerateAndStoreOtp(_foundEmail);
            if (generatedOtp == null)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการสร้าง OTP", "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                return;
            }

            // *** SEND REAL EMAIL ***
            SendOtpEmail(_foundEmail, generatedOtp);
            // ************************

            // (Keep this MessageBox for now to easily see the OTP during testing)
            CustomMessageBoxWindow.Show($"เราได้ส่งรหัส OTP ไปยังอีเมล {_foundEmail} แล้ว กรุณาตรวจสอบ",
                            "ส่ง OTP สำเร็จ",
                            CustomMessageBoxWindow.MessageBoxType.Info);

            // Switch UI and start timer
            inputPanel.Visibility = Visibility.Collapsed;
            otpPanel.Visibility = Visibility.Visible;
            StartCountdown(60); // Start 60 second countdown
        }

        // Generates a 6-digit OTP, saves it with expiry to the DB
        private string GenerateAndStoreOtp(string email)
        {
            string otp = new Random().Next(100000, 999999).ToString("D6");
            DateTime expiryTime = DateTime.Now.AddMinutes(10); // Expires in 10 minutes

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE users SET otp_code = @otp, otp_expiry = @expiry WHERE email = @email";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@otp", otp);
                        cmd.Parameters.AddWithValue("@expiry", expiryTime);
                        cmd.Parameters.AddWithValue("@email", email);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return (rowsAffected > 0) ? otp : null;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึก OTP: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                return null;
            }
        }

        // *** Sends the actual email using MailKit ***
        private void SendOtpEmail(string recipientEmail, string otp)
        {
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("VAMOS Shop (System)", "vamosfootballshop@gmail.com"));
                message.To.Add(new MailboxAddress("", recipientEmail));
                message.Subject = "รหัส OTP สำหรับ VAMOS Shop ของคุณ";

                message.Body = new TextPart("plain")
                {
                    Text = $@"รหัสผ่านครั้งเดียว (OTP) สำหรับ VAMOS Shop ของคุณคือ: {otp}

รหัสนี้จะหมดอายุใน 10 นาที

หากคุณไม่ได้ร้องขอรหัสนี้ โปรดเพิกเฉยอีเมลฉบับนี้"
                };

                using (var client = new SmtpClient())
                {
                    // Use port 465 (SSL) or 587 (StartTls)
                    client.Connect("smtp.gmail.com", 465, SecureSocketOptions.SslOnConnect);

                    // --- IMPORTANT: Authenticate with your Gmail and App Password ---
                    client.Authenticate("vamosfootballshop@gmail.com", "brlravrgllevdnwc"); // <--- REPLACE WITH YOUR APP PASSWORD

                    client.Send(message);
                    client.Disconnect(true);
                    Console.WriteLine($"ส่ง OTP email ไปที่ {recipientEmail} สำเร็จ");
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show($"ไม่สามารถส่ง OTP email ได้: {ex.Message}", "Email Error", CustomMessageBoxWindow.MessageBoxType.Error);
                Console.WriteLine($"Email sending error: {ex.ToString()}");
            }
        }

        // Starts or resets the countdown timer
        private void StartCountdown(int seconds)
        {
            remainingSeconds = seconds;
            tbCountdown.Text = remainingSeconds.ToString();

            // Show countdown, hide resend button
            tbCountdown.Visibility = Visibility.Visible;
            tbCountdown.Siblings().OfType<TextBlock>().ToList().ForEach(tb => tb.Visibility = Visibility.Visible); // Show adjacent text
            btnResendOtp.Visibility = Visibility.Collapsed;

            countdownTimer.Start();
        }

        // Executes every second during countdown
        private void CountdownTimer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            tbCountdown.Text = remainingSeconds.ToString();

            if (remainingSeconds <= 0)
            {
                countdownTimer.Stop();
                // Hide countdown, show resend button
                tbCountdown.Visibility = Visibility.Collapsed;
                tbCountdown.Siblings().OfType<TextBlock>().ToList().ForEach(tb => tb.Visibility = Visibility.Collapsed); // Hide adjacent text
                btnResendOtp.Visibility = Visibility.Visible;
            }
        }

        // User clicks "Resend OTP"
        private void ResendOTP_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(_foundEmail))
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาด: ไม่พบข้อมูลอีเมลเดิม", "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                return;
            }

            // Generate/Store/Send a *new* OTP
            string generatedOtp = GenerateAndStoreOtp(_foundEmail);
            if (generatedOtp == null) { /* Error Handling */ return; }
            SendOtpEmail(_foundEmail, generatedOtp);

            CustomMessageBoxWindow.Show($"ส่ง OTP ใหม่ไปที่ {_foundEmail} แล้ว", "OTP Resent", CustomMessageBoxWindow.MessageBoxType.Info);
            StartCountdown(60);
        }

        // User enters OTP and clicks "VERIFY OTP"
        private void VerifyOTP_Click(object sender, RoutedEventArgs e)
        {
            tbOTPError.Visibility = Visibility.Collapsed;
            string otpInput = txtOTP.Text.Trim();

            if (string.IsNullOrEmpty(otpInput))
            {
                ShowError(tbOTPError, "กรุณากรอก OTP");
                return;
            }

            bool otpVerified = false;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT otp_code, otp_expiry FROM users WHERE email = @email";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", _foundEmail);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedOtp = reader.IsDBNull(reader.GetOrdinal("otp_code")) ? null : reader.GetString("otp_code");
                                DateTime? expiryTime = reader.IsDBNull(reader.GetOrdinal("otp_expiry")) ? (DateTime?)null : reader.GetDateTime("otp_expiry");

                                if (storedOtp == otpInput && expiryTime.HasValue && expiryTime.Value > DateTime.Now)
                                {
                                    otpVerified = true;
                                    ClearOtpAfterVerification(_foundEmail); // Clear OTP after use
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการตรวจสอบ OTP: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                return;
            }

            if (otpVerified)
            {
                countdownTimer.Stop();
                // (เปลี่ยน) ใช้ CustomMessageBoxWindow
                CustomMessageBoxWindow.Show("ยืนยัน OTP สำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);

                if (SlideManage.Instance != null)
                {
                    // Navigate to Reset Password Page
                    SlideManage.Instance.NavigateWithSlide(new ResetPasswordPage(parent, _foundEmail), false);
                }
            }
            else
            {
                ShowError(tbOTPError, "รหัส OTP ไม่ถูกต้อง หรือหมดอายุ");
            }
        }

        // Clears OTP fields in DB after successful verification
        private void ClearOtpAfterVerification(string email)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE users SET otp_code = NULL, otp_expiry = NULL WHERE email = @email";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception) { /* Log or handle error silently */ }
        }

        // --- Back to Login Methods ---
        private void BackToLogin_Click(object sender, MouseButtonEventArgs e)
        {
            BackToLogin();
        }
        private void BackToLogin()
        {
            countdownTimer.Stop(); // Stop timer when going back
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new LoginPage(SlideManage.Instance), true);
            }
        }

        // --- Helper to show error labels ---
        private void ShowError(TextBlock errorLabel, string message)
        {
            errorLabel.Text = message;
            errorLabel.Visibility = Visibility.Visible;
        }
    }

    // Extension method to find sibling controls (used for timer text)
    public static class FrameworkElementExtensions
    {
        public static IEnumerable<DependencyObject> Siblings(this FrameworkElement element)
        {
            if (element?.Parent is Panel parentPanel)
            {
                return parentPanel.Children.OfType<DependencyObject>().Where(child => child != element);
            }
            return Enumerable.Empty<DependencyObject>();
        }
    }
}