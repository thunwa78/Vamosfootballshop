//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;

namespace login_store
{
    // (คลาส AdView เหมือนเดิม)
    public class AdView
    {
        public int AdId { get; set; }
        public string ImagePath { get; set; }
        public string TargetUrl { get; set; }
        public int SortOrder { get; set; }
        public bool IsActive { get; set; }

        public string StatusText => IsActive ? "✅ เปิดใช้งาน" : "❌ ปิดใช้งาน";
        public string ToggleText => IsActive ? "ปิด" : "เปิด";
        public Brush ToggleColor => IsActive ? Brushes.DarkRed : Brushes.Green;
    }

    public partial class AdminManageAdsPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private List<AdView> allAds = new List<AdView>();

        public AdminManageAdsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAds();
        }

        // --- 1. โหลดรายการโฆษณา (เมธอด LoadAds) ---
        public void LoadAds() // ทำให้เป็น public เพื่อเรียกใช้จากหน้า Add/Edit
        {
            allAds.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT ad_id, image_path, target_url, sort_order, is_active FROM ad_slides ORDER BY sort_order ASC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allAds.Add(new AdView
                            {
                                AdId = reader.GetInt32("ad_id"),
                                ImagePath = reader.GetString("image_path"),
                                TargetUrl = reader.IsDBNull(reader.GetOrdinal("target_url")) ? "-" : reader.GetString("target_url"),
                                SortOrder = reader.GetInt32("sort_order"),
                                IsActive = reader.GetBoolean("is_active")
                            });
                        }
                    }
                    AdsDataGrid.ItemsSource = null;
                    AdsDataGrid.ItemsSource = allAds;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลโฆษณา: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 2. ปุ่มเพิ่มโฆษณา (Add) ---
        private void btnAddAd_Click(object sender, RoutedEventArgs e)
        {
            var addWindow = new AddEditAdWindow();
            if (addWindow.ShowDialog() == true)
            {
                LoadAds();
            }
        }

        // --- 3. ปุ่มแก้ไขโฆษณา (Edit) ---
        private void btnEditAd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adId)
            {
                var editWindow = new AddEditAdWindow(adId);
                if (editWindow.ShowDialog() == true)
                {
                    LoadAds();
                }
            }
        }

        private void btnDeleteAd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adId)
            {
                MessageBoxResult result = MessageBox.Show(
                    $"คุณแน่ใจหรือไม่ว่าต้องการลบโฆษณา ID: {adId} ?",
                    "ยืนยันการลบ",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning
                );

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sql = "DELETE FROM ad_slides WHERE ad_id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", adId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        CustomMessageBoxWindow.Show($"ลบโฆษณา ID: {adId} สำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                        LoadAds(); // รีโหลดข้อมูลใหม่
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBoxWindow.Show($"เกิดข้อผิดพลาดในการลบโฆษณา: {ex.Message}", "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                    }
                }
            }
        }


        // --- 4. ปุ่มเปิด/ปิด (Toggle Active) ---
        private void btnToggleAd_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int adId)
            {
                var ad = allAds.Find(a => a.AdId == adId);
                if (ad == null) return;

                bool newStatus = !ad.IsActive;
                string action = newStatus ? "เปิดใช้งาน" : "ปิดใช้งาน";

                MessageBoxResult result = MessageBox.Show($"คุณแน่ใจหรือไม่ว่าต้องการ {action} โฆษณา ID: {adId}?", $"ยืนยันการ{action}", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sql = "UPDATE ad_slides SET is_active = @status WHERE ad_id = @adId";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@status", newStatus);
                                cmd.Parameters.AddWithValue("@adId", adId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        CustomMessageBoxWindow.Show($"{action}โฆษณา ID: {adId} สำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                        LoadAds(); // รีเฟรชตาราง
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBoxWindow.Show($"เกิดข้อผิดพลาดในการ{action}โฆษณา: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                    }
                }
            }
        }
    }
}