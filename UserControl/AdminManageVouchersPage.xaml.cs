//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace login_store
{
    // (Model VoucherView - เหมือนเดิม)
    public class VoucherView : Voucher
    {
        public string StatusText => IsActive ? "Active" : "Disabled";
        public string ToggleText => IsActive ? "ปิด" : "เปิด";
        public Brush ToggleColor => IsActive ? Brushes.Gray : Brushes.Green;
    }


    public partial class AdminManageVouchersPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private ObservableCollection<VoucherView> allVouchers = new ObservableCollection<VoucherView>();

        public AdminManageVouchersPage()
        {
            InitializeComponent();
            VouchersDataGrid.ItemsSource = allVouchers;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadVouchers();
        }

        // --- 1. โหลดข้อมูล (เหมือนเดิม) ---
        private void LoadVouchers()
        {
            allVouchers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM vouchers ORDER BY valid_to DESC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allVouchers.Add(new VoucherView
                            {
                                VoucherId = reader.GetInt32("voucher_id"),
                                Code = reader.GetString("code"),
                                Description = reader.GetString("description"),
                                DiscountAmount = reader.GetDecimal("discount_amount"),

                                // (สำคัญ) 👈 ต้องอ่านค่า discount_type มาด้วย
                                DiscountType = reader.IsDBNull(reader.GetOrdinal("discount_type")) ? "Fixed" : reader.GetString("discount_type"),

                                MinPurchase = reader.GetDecimal("min_purchase"),
                                ValidFrom = reader.GetDateTime("valid_from"),
                                ValidTo = reader.GetDateTime("valid_to"),
                                IsActive = reader.GetBoolean("is_active")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลโค้ดส่วนลด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) 👈 1. แก้ไขปุ่ม "เพิ่ม"
        private void btnAddVoucher_Click(object sender, RoutedEventArgs e)
        {
            // (แก้) เปิดหน้าต่าง AddEditVoucherWindow (โหมดเพิ่ม)
            AddEditVoucherWindow addWindow = new AddEditVoucherWindow();

            // ถ้า User กด "บันทึก" (DialogResult = true) ให้โหลดตารางใหม่
            if (addWindow.ShowDialog() == true)
            {
                LoadVouchers();
            }
        }

        // (แก้) 👈 2. แก้ไขปุ่ม "แก้ไข"
        private void btnEditVoucher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int voucherId)
            {
                // (แก้) เปิดหน้าต่าง AddEditVoucherWindow (โหมดแก้ไข)
                AddEditVoucherWindow editWindow = new AddEditVoucherWindow(voucherId);

                if (editWindow.ShowDialog() == true)
                {
                    LoadVouchers();
                }
            }
        }

        // --- 4. ปุ่มเปิด/ปิด (เหมือนเดิม) ---
        private void btnToggleVoucher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is VoucherView voucher)
            {
                bool newStatus = !voucher.IsActive;
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "UPDATE vouchers SET is_active = @status WHERE voucher_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@status", newStatus);
                            cmd.Parameters.AddWithValue("@id", voucher.VoucherId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadVouchers();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการอัปเดตสถานะ: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- 5. ปุ่มลบ (เหมือนเดิม) ---
        private void btnDeleteVoucher_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int voucherId)
            {
                if (MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบโค้ดนี้?\n(การลบนี้จะลบโค้ดออกจาก 'โค้ดของฉัน' ของ User ทุกคนด้วย)", "ยืนยันการลบ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sql = "DELETE FROM vouchers WHERE voucher_id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", voucherId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadVouchers();
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบ: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                    }
                }
            }
        }
    }
}