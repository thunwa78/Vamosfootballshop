//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Globalization; // (เพิ่ม)
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data; // (เพิ่ม)
using System.Windows.Input;

namespace login_store
{
    public partial class NotificationsPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;

        public NotificationsPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadNotifications();
        }

        private void LoadNotifications()
        {
            List<Notification> notificationList = new List<Notification>();
            txtNoNotifications.Visibility = Visibility.Collapsed;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT notification_id, user_id, message, is_read, created_at FROM notifications " +
                                 "WHERE user_id = @user_id ORDER BY created_at DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                notificationList.Add(new Notification
                                {
                                    NotificationId = reader.GetInt32("notification_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    Message = reader.GetString("message"),
                                    IsRead = reader.GetBoolean("is_read"),
                                    CreatedAt = reader.GetDateTime("created_at")
                                });
                            }
                        }
                    }
                }

                NotificationsListBox.ItemsSource = notificationList;

                if (notificationList.Count == 0)
                {
                    txtNoNotifications.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดการแจ้งเตือน: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void btnMarkOneRead_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int notificationId)
            {
                MarkAsRead(notificationId);
            }
        }

        private void btnMarkAllRead_Click(object sender, RoutedEventArgs e)
        {
            MarkAsRead(null);
        }

        private void MarkAsRead(int? notificationId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;

                    if (notificationId.HasValue)
                    {
                        sql = "UPDATE notifications SET is_read = 1 WHERE notification_id = @id AND user_id = @user_id";
                    }
                    else
                    {
                        sql = "UPDATE notifications SET is_read = 1 WHERE user_id = @user_id AND is_read = 0";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        if (notificationId.HasValue)
                        {
                            cmd.Parameters.AddWithValue("@id", notificationId.Value);
                        }
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        cmd.ExecuteNonQuery();
                    }
                }
                LoadNotifications();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการอัปเดต: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- Window Control Methods & Top Nav Bar ---

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
            if (e.Source == sender) { Window.GetWindow(this)?.DragMove(); }
        }
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            btnShop_Click(sender, e);
        }
        private void btnShop_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ShopPage(parent), true);
        }
        private void btnCart_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new CartPage(parent), true);
        }
        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), false);
        }
        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new MyVouchersPage(parent), true);
            }
        }
        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            // (เราอยู่ที่นี่แล้ว ไม่ต้องทำอะไร)
        }
        private void btnWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), false);
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new UserProfilePage(parent), true);
        }
        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                // (เรียกใช้หน้าที่เราเพิ่งสร้าง)
                SlideManage.Instance.NavigateWithSlide(new AboutUsPage(parent), false);
            }
        }
    }


    // --- (ใหม่) คลาส Converter ที่ใช้กลับค่า (Invert) Boolean ---
    // (true -> Collapsed, false -> Visible)
    public class InvertedBooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                // ถ้า true (อ่านแล้ว) -> ให้ Collapsed (ซ่อน)
                // ถ้า false (ยังไม่อ่าน) -> ให้ Visible (แสดง)
                return boolValue ? Visibility.Collapsed : Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}