using System;
using System.Windows;
using System.Windows.Media.Imaging; // เพิ่ม using นี้

namespace login_store
{
    public partial class CustomMessageBoxWindow : Window
    {
        // Enum สำหรับประเภท Icon (ทางเลือก)
        public enum MessageBoxType
        {
            Success,
            Error,
            Info,
            Warning
        }

        // Constructor ส่วนตัว
        private CustomMessageBoxWindow(string title, string message, MessageBoxType type)
        {
            InitializeComponent();
            txtTitle.Text = title;
            txtMessage.Text = message;

            // ตั้งค่า Icon และสีปุ่มตาม Type
            switch (type)
            {
                case MessageBoxType.Success:
                    imgIcon.Source = LoadBitmapImage("/Images/check_icon.png"); // ไอคอนติ๊กถูก
                    btnOk.Background = System.Windows.Media.Brushes.Green; // ปุ่มสีเขียว
                    break;
                case MessageBoxType.Error:
                    imgIcon.Source = LoadBitmapImage("/Images/error_icon.png"); // ไอคอนกากบาท (ต้องหามาใส่)
                    btnOk.Background = System.Windows.Media.Brushes.Red; // ปุ่มสีแดง
                    txtTitle.Text = string.IsNullOrEmpty(title) || title == "Success" ? "Error" : title; // เปลี่ยน Title ถ้ายังเป็น Success
                    break;
                case MessageBoxType.Info:
                    imgIcon.Source = LoadBitmapImage("/Images/info_icon.png"); // ไอคอนตัว i (ต้องหามาใส่)
                    btnOk.Background = System.Windows.Media.Brushes.DodgerBlue; // ปุ่มสีฟ้า
                    txtTitle.Text = string.IsNullOrEmpty(title) || title == "Success" ? "Information" : title;
                    break;
                case MessageBoxType.Warning:
                    imgIcon.Source = LoadBitmapImage("/Images/warning_icon.png"); // ไอคอนตกใจ (ต้องหามาใส่)
                    btnOk.Background = System.Windows.Media.Brushes.Orange; // ปุ่มสีส้ม
                    txtTitle.Text = string.IsNullOrEmpty(title) || title == "Success" ? "Warning" : title;
                    break;
            }
        }

        // เมธอด static สำหรับเรียกใช้งาน (แบบง่าย)
        public static void Show(string message, string title = "Success", MessageBoxType type = MessageBoxType.Success)
        {
            var customBox = new CustomMessageBoxWindow(title, message, type);
            customBox.ShowDialog();
        }

        // เมธอดช่วยโหลด BitmapImage (ป้องกัน Error ถ้าไฟล์ไม่เจอ)
        private BitmapImage LoadBitmapImage(string relativeUri)
        {
            try
            {
                return new BitmapImage(new Uri(relativeUri, UriKind.Relative));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading image {relativeUri}: {ex.Message}");
                return null; // หรือ return default icon
            }
        }

        // ปุ่ม OK
        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        // ปุ่มปิด X
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}