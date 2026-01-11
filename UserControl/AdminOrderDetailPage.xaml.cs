//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace login_store
{
    public partial class AdminOrderDetailPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int orderId;
        private int customerId; // (ไว้ใช้ดึงที่อยู่)

        // (เราใช้ Model 'CartItemDetails' ซ้ำจากหน้า CartPage ได้เลย)
        private ObservableCollection<CartItemDetails> orderItems = new ObservableCollection<CartItemDetails>();

        public AdminOrderDetailPage(int orderId)
        {
            InitializeComponent();
            this.orderId = orderId;
            ItemsDataGrid.ItemsSource = orderItems;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrderDetails();
            LoadOrderItems();
        }

        // "ตัวช่วย" โหลดรูปภาพ
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

        // (ดึงข้อมูลหัวบิล)
        private void LoadOrderDetails()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (Join ตาราง orders, users, และ vouchers)
                    string sql = @"
                        SELECT o.order_id, o.user_id, u.username, u.email, 
                               o.total_amount, o.discount_amount, v.code AS voucher_code
                        FROM orders o
                        JOIN users u ON o.user_id = u.id
                        LEFT JOIN vouchers v ON o.used_voucher_id = v.voucher_id
                        WHERE o.order_id = @order_id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@order_id", this.orderId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                this.customerId = reader.GetInt32("user_id"); // 👈 เก็บ ID ลูกค้า
                                txtTitle.Text = $"รายละเอียดออเดอร์ #{reader.GetInt32("order_id")}";
                                txtCustomerName.Text = GetStringSafe(reader, "username");
                                txtCustomerEmail.Text = GetStringSafe(reader, "email");
                                txtTotalAmount.Text = $"Total: {reader.GetDecimal("total_amount"):N2}";
                                txtDiscount.Text = $"Discount: {reader.GetDecimal("discount_amount"):N2}";
                                txtVoucher.Text = $"Voucher: {GetStringSafe(reader, "voucher_code", "-")}";
                            }
                        }
                    }
                }

                // (หลังจากรู้ ID ลูกค้าแล้ว ค่อยไปดึงที่อยู่)
                LoadCustomerDefaultAddress();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลหัวบิล: " + ex.Message, "Error");
            }
        }

        // (ดึงที่อยู่ Default ของลูกค้า)
        private void LoadCustomerDefaultAddress()
        {
            if (this.customerId == 0) return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM user_addresses WHERE user_id = @userId ORDER BY is_default DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.customerId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtShippingAddress.Text = $"{GetStringSafe(reader, "address_line1")}, {GetStringSafe(reader, "sub_district")}, {GetStringSafe(reader, "district")}, {GetStringSafe(reader, "province")} {GetStringSafe(reader, "postal_code")}";
                                txtShippingPhone.Text = GetStringSafe(reader, "phone_number");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtShippingAddress.Text = "Error: " + ex.Message;
            }
        }

        // (ดึงรายการสินค้า)
        private void LoadOrderItems()
        {
            orderItems.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (SQL JOIN 3 ตาราง)
                    string sqlItems = @"
                        SELECT 
                            p.name, p.image_path, pv.size_name, oi.quantity, oi.price_at_purchase 
                        FROM order_items oi 
                        LEFT JOIN product_variants pv ON oi.product_variant_id = pv.variant_id
                        LEFT JOIN products p ON p.product_id = COALESCE(pv.product_id, oi.product_id)
                        WHERE oi.order_id = @order_id";

                    using (MySqlCommand cmdItems = new MySqlCommand(sqlItems, conn))
                    {
                        cmdItems.Parameters.AddWithValue("@order_id", this.orderId);
                        using (var reader = cmdItems.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string imgPath = GetStringSafe(reader, "image_path");
                                orderItems.Add(new CartItemDetails
                                {
                                    Name = reader.GetString("name"),
                                    SizeName = GetStringSafe(reader, "size_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    Price = reader.GetDecimal("price_at_purchase"),
                                    ImagePath = imgPath,
                                    ImagePreview = LoadImagePreview(imgPath) // 👈 โหลดพรีวิว
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดรายการสินค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (Helper Methods)
        private string GetStringSafe(MySqlDataReader reader, string columnName, string defaultVal = "")
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex)) return defaultVal;
            return reader.GetString(colIndex);
        }

        private void btnGoBack_Click(object sender, RoutedEventArgs e)
        {
            if (this.NavigationService.CanGoBack)
            {
                this.NavigationService.GoBack();
            }
        }
    }
}