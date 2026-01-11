//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace login_store
{
    // (เพิ่ม) 1. Model สำหรับแสดงผลใน ListBox
    // (เราใช้คลาส Voucher ที่เราสร้างไว้ มา "ต่อยอด")
    public class VoucherDisplay : Voucher
    {
        // Property สำหรับหน้า "ศูนย์รวมโค้ด"
        public bool CanCollect { get; set; } = true; // (Default: เก็บได้)

        public string CollectButtonText => CanCollect ? "เก็บโค้ด" : "เก็บแล้ว";
        public Brush CollectButtonColor => CanCollect ? (Brush)new BrushConverter().ConvertFrom("#3498DB") : Brushes.Gray;

        // Property สำหรับหน้า "โค้ดของฉัน"
        public string MinPurchaseText => (MinPurchase > 0) ? $"ขั้นต่ำ {MinPurchase:N0} บาท" : "ไม่มีขั้นต่ำ";
        public string ValidToText => $"ใช้ได้ถึง: {ValidTo:dd/MM/yyyy}";
    }


    public partial class MyVouchersPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;

        // (เพิ่ม) 2. Collections สำหรับผูกข้อมูลกับ ListBox
        private ObservableCollection<VoucherDisplay> myVouchers = new ObservableCollection<VoucherDisplay>();
        private ObservableCollection<VoucherDisplay> allVouchers = new ObservableCollection<VoucherDisplay>();

        public MyVouchersPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;

            // (เพิ่ม) 3. ผูก ItemsSource
            MyVouchersListBox.ItemsSource = myVouchers;
            AllVouchersListBox.ItemsSource = allVouchers;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadMyVouchers();
            LoadAllVouchers();
        }

        // --- 4. โหลด "โค้ดของฉัน" (ที่เก็บมาแล้ว) ---
        private void LoadMyVouchers()
        {
            myVouchers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (SQL) Join ตาราง `user_vouchers` (ของฉัน) กับ `vouchers` (โค้ดหลัก)
                    string sql = @"
                        SELECT v.*
                        FROM user_vouchers uv
                        JOIN vouchers v ON uv.voucher_id = v.voucher_id
                        WHERE uv.user_id = @userId 
                          AND uv.is_used = 0 
                          AND v.valid_to > NOW()"; // (ดึงเฉพาะที่ยังไม่หมดอายุและยังไม่ใช้)

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                myVouchers.Add(new VoucherDisplay
                                {
                                    VoucherId = reader.GetInt32("voucher_id"),
                                    Code = reader.GetString("code"),
                                    Description = reader.GetString("description"),
                                    DiscountAmount = reader.GetDecimal("discount_amount"),
                                    MinPurchase = reader.GetDecimal("min_purchase"),
                                    ValidTo = reader.GetDateTime("valid_to")
                                });
                            }
                        }
                    }
                }
                // (แสดง/ซ่อน Text "ไม่พบโค้ด")
                txtNoMyVouchers.Visibility = (myVouchers.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลด 'โค้ดของฉัน': " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 5. โหลด "โค้ดทั้งหมด" (ที่มีให้เก็บ) ---
        private void LoadAllVouchers()
        {
            allVouchers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (SQL) ดึงโค้ดทั้งหมด และ "ตรวจสอบ" (LEFT JOIN) ว่า User คนนี้เก็บไปแล้วหรือยัง (uv.user_voucher_id)
                    string sql = @"
                        SELECT v.*, uv.user_voucher_id
                        FROM vouchers v
                        LEFT JOIN user_vouchers uv ON v.voucher_id = uv.voucher_id AND uv.user_id = @userId
                        WHERE v.is_active = 1 
                          AND v.valid_to > NOW()"; // (ดึงเฉพาะที่ยังไม่หมดอายุและ Admin เปิดใช้งาน)

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allVouchers.Add(new VoucherDisplay
                                {
                                    VoucherId = reader.GetInt32("voucher_id"),
                                    Code = reader.GetString("code"),
                                    Description = reader.GetString("description"),
                                    DiscountAmount = reader.GetDecimal("discount_amount"),
                                    MinPurchase = reader.GetDecimal("min_purchase"),
                                    ValidTo = reader.GetDateTime("valid_to"),
                                    // (สำคัญ) เช็กว่าเก็บไปหรือยัง
                                    CanCollect = reader.IsDBNull(reader.GetOrdinal("user_voucher_id"))
                                });
                            }
                        }
                    }
                }
                // (แสดง/ซ่อน Text "ไม่พบโค้ด")
                txtNoAllVouchers.Visibility = (allVouchers.Count == 0) ? Visibility.Visible : Visibility.Collapsed;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลด 'ศูนย์รวมโค้ด': " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 6. Event Handlers ---

        private void btnUseVoucher_Click(object sender, RoutedEventArgs e)
        {
            // (กด "ใช้เลย" -> พาไปหน้า Shop)
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ShopPage(parent), true);
        }

        private void btnCollectVoucher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int voucherId)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        // (INSERT ลงตาราง `user_vouchers`)
                        string sql = "INSERT INTO user_vouchers (user_id, voucher_id) VALUES (@userId, @voucherId)";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                            cmd.Parameters.AddWithValue("@voucherId", voucherId);
                            cmd.ExecuteNonQuery();
                        }
                    }

                    CustomMessageBoxWindow.Show("เก็บโค้ดส่วนลดสำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);

                    // (สำคัญ) โหลดข้อมูลทั้ง 2 แท็บใหม่
                    LoadMyVouchers();
                    LoadAllVouchers();
                }
                catch (MySqlException ex) when (ex.Number == 1062) // Error โค้ดซ้ำ
                {
                    CustomMessageBoxWindow.Show("คุณเก็บโค้ดนี้ไปแล้ว", "Info", CustomMessageBoxWindow.MessageBoxType.Info);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการเก็บโค้ด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- 7. เมธอดควบคุมหน้าต่าง และ Top Nav Bar (เหมือนเดิม) ---

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
            btnShop_Click(sender, new RoutedEventArgs());
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
                SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), true);
        }
        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
           
        }
        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new NotificationsPage(parent), false);
        }
        private void btnWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), true);
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
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.EndSession();
            if (parent != null)
            {
                parent.NavigateWithSlide(new LoginPage(parent), true);
            }
        }
    }
}