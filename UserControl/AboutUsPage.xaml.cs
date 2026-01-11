using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;
using MySqlConnector; // (หรือ MySql.Data.MySqlClient)
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using System.Collections.ObjectModel; // 👈 (สำคัญ) ต้องใช้สำหรับ ListBox

namespace login_store
{
    public partial class AboutUsPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        // (เพิ่ม) 👈 1. Collection สำหรับผูกข้อมูลกับ ListBox ทีม
        private ObservableCollection<TeamMember> teamMembers = new ObservableCollection<TeamMember>();

        public AboutUsPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;

            // (เพิ่ม) 👈 2. ผูก ListBox กับ Collection
            TeamListBox.ItemsSource = teamMembers;
        }

        // (เพิ่ม) 👈 3. เมธอด Loaded (จะถูกเรียกโดย XAML)
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCompanyInfo();
            LoadTeamMembers();
        }

        // --- 4. "ตัวช่วย" โหลดรูปภาพ (คัดลอกจากหน้าอื่น) ---
        private BitmapImage LoadImagePreview(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath)) return null;
            try
            {
                string baseDir = AppContext.BaseDirectory;
                string path = relativePath.TrimStart('/');
                string fullPath = Path.Combine(baseDir, path);

                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception) { /* Handle error */ }
            return null;
        }

        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex)) return string.Empty;
            return reader.GetString(colIndex);
        }

        // --- 5. โหลดข้อมูลหลัก (จากตาราง company_info) ---
        private void LoadCompanyInfo()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM company_info WHERE id = 1 LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            // (นำข้อมูลไปใส่ใน TextBlock ที่เราตั้งชื่อไว้)
                            txtDescription.Text = GetStringSafe(reader, "description");
                            txtEmail.Text = GetStringSafe(reader, "email");
                            txtPhone.Text = GetStringSafe(reader, "phone");
                            txtAddress.Text = GetStringSafe(reader, "address");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลหน้า About Us: " + ex.Message, "Database Error");
            }
        }

        // --- 6. โหลดข้อมูลทีม (จากตาราง team_members) ---
        private void LoadTeamMembers()
        {
            teamMembers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM team_members ORDER BY sort_order ASC, name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string path = GetStringSafe(reader, "image_path");
                            teamMembers.Add(new TeamMember
                            {
                                MemberId = reader.GetInt32("member_id"),
                                Name = GetStringSafe(reader, "name"),
                                Role = GetStringSafe(reader, "role"),
                                ImagePath = path,
                                // (โหลดพรีวิวรูป)
                                ImagePreview = LoadImagePreview(path)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลทีม: " + ex.Message, "Database Error");
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
            if (e.OriginalSource.GetType() == typeof(Border) || e.OriginalSource.GetType() == typeof(Grid) || e.OriginalSource.GetType() == typeof(ScrollViewer))
            {
                Window.GetWindow(this)?.DragMove();
            }
        }
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            btnShop_Click(sender, e);
        }
        private void btnShop_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ShopPage(parent), true); // สไลด์กลับ
        }
        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), false); // สไลด์ไปหน้า
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.EndSession();
            if (parent != null)
            {
                parent.NavigateWithSlide(new LoginPage(parent), true);
            }
        }
        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new MyVouchersPage(parent), false);
            }
        }
        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new NotificationsPage(parent), false);
        }
        private void btnWishlist_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), false);
        }
        private void btnCart_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new CartPage(parent), false);
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new UserProfilePage(parent), false);
        }

        // (เพิ่ม) 👈 เมธอดใหม่ เมื่อ User คลิกที่รูปทีม
        private void TeamMember_Click(object sender, MouseButtonEventArgs e)
        {
            // (ตรวจสอบว่าสิ่งที่ถูกคลิกคือ StackPanel และ Tag เก็บข้อมูล TeamMember อยู่)
            if (sender is StackPanel stackPanel && stackPanel.Tag is TeamMember member)
            {
                // (สร้างและเปิดหน้าต่าง TeamMemberDetailWindow)
                TeamMemberDetailWindow detailWindow = new TeamMemberDetailWindow(member);
                detailWindow.ShowDialog(); // (ShowDialog ทำให้หน้าต่างหลักรอ)
            }
        }

        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
         //อยู่หน้านี้อยู่แล้วอิอิ
        }

        private void txtTaxId_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}