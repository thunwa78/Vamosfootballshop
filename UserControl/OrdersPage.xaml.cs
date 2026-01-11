using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace login_store
{
    public class OrderDetails
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }

        public decimal DiscountAmount { get; set; }

        public string VoucherCode { get; set; }
    }

    public partial class OrdersPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;

        public OrdersPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrders();
        }

        private void LoadOrders()
        {
            List<OrderDetails> ordersList = new List<OrderDetails>();
            txtNoOrders.Visibility = Visibility.Collapsed;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT order_id, order_date, total_amount, status " +
                                 "FROM orders " +
                                 "WHERE user_id = @user_id " +
                                 "ORDER BY order_date DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ordersList.Add(new OrderDetails
                                {
                                    OrderId = reader.GetInt32("order_id"),
                                    OrderDate = reader.GetDateTime("order_date"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    Status = reader.GetString("status")
                                });
                            }
                        }
                    }
                }

                OrdersListBox.ItemsSource = ordersList;

                if (ordersList.Count == 0)
                {
                    txtNoOrders.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดประวัติการสั่งซื้อได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                if (SlideManage.Instance != null)
                {
                    SlideManage.Instance.NavigateWithSlide(new OrderDetailPage(parent, orderId), false);
                }
            }
        }

        // --- (แก้) เมธอดควบคุมหน้าต่าง และ Top Nav Bar ---

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
            if (e.Source == sender)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        // --- (Top Nav) ---
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
            // (เราอยู่ที่นี่แล้ว)
        }
        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new NotificationsPage(parent), true);
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
        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new MyVouchersPage(parent), true);
            }
        }
    }
}