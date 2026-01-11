//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using System.IO;
using System.Diagnostics; // 👈 (เพิ่ม)

namespace login_store
{
    // (คลาส AdminOrderView - เหมือนเดิม)
    public class AdminOrderView
    {
        public int OrderId { get; set; }
        public string Username { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public int UserId { get; set; }
        public string SlipPath { get; set; }
        public Visibility ShowSlipButton => string.IsNullOrEmpty(SlipPath) ? Visibility.Collapsed : Visibility.Visible;
    }

    public partial class AdminManageOrdersPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private string currentFilter = "";

        // (เพิ่ม) 👈 1. เพิ่มตัวแปรนี้
        private int filterByUserId = 0;

        // (Constructor เดิม - สำหรับรับ Status)
        public AdminManageOrdersPage(string initialFilter = "")
        {
            InitializeComponent();
            this.currentFilter = initialFilter;
        }

        // (เพิ่ม) 👈 2. เพิ่ม Constructor ใหม่นี้ทั้งหมด (สำหรับรับ User ID)
        public AdminManageOrdersPage(int userId)
        {
            InitializeComponent();
            this.filterByUserId = userId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // (โค้ดเดิมของคุณ)
            LoadOrders();

            // (เพิ่ม) 👈 3. เพิ่มโค้ดนี้ เพื่อแสดงปุ่ม "ย้อนกลับ"
            if (filterByUserId > 0)
            {
                btnGoBack.Visibility = Visibility.Visible;
            }
        }

        

        // (แก้) 👈 4. แก้ไขเมธอดนี้ใหม่ทั้งหมด (ลบ txtSearch ออก)
        private void LoadOrders()
        {
            List<AdminOrderView> ordersList = new List<AdminOrderView>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT o.order_id, u.username, o.order_date, o.total_amount, o.status, o.user_id, o.slip_path " +
                                 "FROM orders o " +
                                 "JOIN users u ON o.user_id = u.id " +
                                 "WHERE 1=1 "; // 👈 (แก้) เริ่มด้วย WHERE 1=1

                    // (แก้) 👈 เพิ่ม Logic การกรอง
                    if (filterByUserId > 0)
                    {
                        sql += "AND o.user_id = @user_id ";
                    }
                    else if (!string.IsNullOrEmpty(currentFilter))
                    {
                        sql += "AND o.status = @status ";
                    }

                    sql += "ORDER BY o.order_date DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        // (แก้) 👈 เพิ่มการ Bind Parameter ให้ครบ
                        if (filterByUserId > 0)
                        {
                            cmd.Parameters.AddWithValue("@user_id", filterByUserId);
                        }
                        else if (!string.IsNullOrEmpty(currentFilter))
                        {
                            cmd.Parameters.AddWithValue("@status", currentFilter);
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ordersList.Add(new AdminOrderView
                                {
                                    OrderId = reader.GetInt32("order_id"),
                                    Username = reader.GetString("username"),
                                    OrderDate = reader.GetDateTime("order_date"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    Status = reader.GetString("status"),
                                    UserId = reader.GetInt32("user_id"),
                                    SlipPath = reader.IsDBNull(reader.GetOrdinal("slip_path")) ? null : reader.GetString("slip_path")
                                });
                            }
                        }
                    }
                }
                OrdersDataGrid.ItemsSource = ordersList;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลคำสั่งซื้อได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (เพิ่ม) 👈 5. เพิ่มเมธอดนี้สำหรับปุ่มย้อนกลับ
        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            // สั่งให้ NavigationService (บริการนำทาง)
            // ย้อนกลับไปหน้าล่าสุด (ซึ่งก็คือหน้า AdminManageUsersPage)
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }

        private void btnViewDetails_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                // (แก้) 👈 1. ตรวจสอบว่าหน้านี้มี NavigationService หรือไม่
                if (this.NavigationService != null)
                {
                    // (แก้) 👈 2. สั่งให้ NavigationService เปิดหน้า Orders
                    this.NavigationService.Navigate(new AdminOrderDetailPage(orderId));
                }
                else
                {
                    // (กรณีฉุกเฉิน ถ้าหา Service ไม่เจอ)
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาด: ไม่พบ NavigationService", "Navigation Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- 1. Event: ดูสลิป (View Slip) (เหมือนเดิม) ---
        private void btnViewSlip_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string slipPath)
            {
                if (string.IsNullOrEmpty(slipPath)) return;
                try
                {
                    string combinedPath = System.IO.Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        slipPath.TrimStart('/', '\\')
                    );

                    if (System.IO.File.Exists(combinedPath))
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(combinedPath) { UseShellExecute = true });
                    }
                    else
                    {
                        CustomMessageBoxWindow.Show($"ไม่พบไฟล์สลิป: {combinedPath}", "File Not Found", CustomMessageBoxWindow.MessageBoxType.Error);
                    }
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("ไม่สามารถเปิดไฟล์รูปภาพ: " + ex.Message, "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- 2. Event: อนุมัติ (Approve) (เหมือนเดิม) ---
        private void btnApprove_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                AdminOrderView orderToApprove = (OrdersDataGrid.ItemsSource as List<AdminOrderView>)?.Find(o => o.OrderId == orderId);

                // (ผมใช้ 'Pending Approval' ตาม XAML ของคุณ)
                if (orderToApprove == null || orderToApprove.Status != "Pending Approval") return;

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sqlUpdate = "UPDATE orders SET status = 'Approved' WHERE order_id = @order_id";
                        using (MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdate, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@order_id", orderId);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        string notificationMsg = $"การชำระเงินสำหรับ Order #{orderId} ได้รับการอนุมัติแล้ว และสินค้าอยู่ในขั้นตอนการจัดส่ง";
                        string sqlNotify = "INSERT INTO notifications (user_id, message, is_read) VALUES (@user_id, @message, 0)";
                        using (MySqlCommand cmdNotify = new MySqlCommand(sqlNotify, conn))
                        {
                            cmdNotify.Parameters.AddWithValue("@user_id", orderToApprove.UserId);
                            cmdNotify.Parameters.AddWithValue("@message", notificationMsg);
                            cmdNotify.ExecuteNonQuery();
                        }
                    }

                    CustomMessageBoxWindow.Show($"อนุมัติ Order #{orderId} สำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    LoadOrders();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการอนุมัติ: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- 3. Event: ปฏิเสธ (Reject) (เหมือนเดิม) ---
        private void btnReject_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int orderId)
            {
                AdminOrderView orderToReject = (OrdersDataGrid.ItemsSource as List<AdminOrderView>)?.Find(o => o.OrderId == orderId);

                // (ผมใช้ 'Pending Approval' ตาม XAML ของคุณ)
                if (orderToReject == null || orderToReject.Status != "Pending Approval") return;

                MessageBoxResult confirm = MessageBox.Show($"คุณแน่ใจหรือไม่ว่าต้องการปฏิเสธ Order #{orderId}?", "ยืนยันการปฏิเสธ", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (confirm == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sqlUpdate = "UPDATE orders SET status = 'Cancelled' WHERE order_id = @order_id";
                            using (MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdate, conn))
                            {
                                cmdUpdate.Parameters.AddWithValue("@order_id", orderId);
                                cmdUpdate.ExecuteNonQuery();
                            }

                            string notificationMsg = $"Order #{orderId} ถูกปฏิเสธ เนื่องจากหลักฐานการโอนเงินไม่ถูกต้อง";
                            string sqlNotify = "INSERT INTO notifications (user_id, message, is_read) VALUES (@user_id, @message, 0)";
                            using (MySqlCommand cmdNotify = new MySqlCommand(sqlNotify, conn))
                            {
                                cmdNotify.Parameters.AddWithValue("@user_id", orderToReject.UserId);
                                cmdNotify.Parameters.AddWithValue("@message", notificationMsg);
                                cmdNotify.ExecuteNonQuery();
                            }
                        }

                        CustomMessageBoxWindow.Show($"ปฏิเสธ Order #{orderId} สำเร็จ! สถานะถูกเปลี่ยนเป็น Cancelled", "Rejected", CustomMessageBoxWindow.MessageBoxType.Success);
                        LoadOrders();
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการปฏิเสธ: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                    }
                }
            }
        }

        // (แก้) 👈 6. แก้ไขเมธอดนี้
        private void FilterButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                currentFilter = button.Tag.ToString();
                filterByUserId = 0; // (สำคัญ) 👈 รีเซ็ตการกรอง User ID
                LoadOrders();
            }
        }
    }
}