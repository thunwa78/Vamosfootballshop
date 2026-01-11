using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace login_store
{
    // Model (เหมือนเดิม)
    public class ProductDisplayModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int? BrandId { get; set; }
        public string BrandName { get; set; }
        public int StockQuantity { get; set; }
        public bool IsActive { get; set; }
    }

    public partial class AdminManageProductsPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private ObservableCollection<ProductDisplayModel> allProducts = new ObservableCollection<ProductDisplayModel>();
        private ObservableCollection<ProductDisplayModel> filteredProducts = new ObservableCollection<ProductDisplayModel>();

        public AdminManageProductsPage()
        {
            InitializeComponent();
            ProductsDataGrid.ItemsSource = filteredProducts;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProducts();
        }

        // --- 1. โหลดข้อมูลสินค้า (เหมือนเดิม) ---
        public void LoadProducts()
        {
            allProducts.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            p.product_id, p.name, p.price, p.stock_quantity, p.image_path, 
                            p.category_id, pc.name AS category_name, 
                            p.brand_id, pb.name AS brand_name, p.is_active
                        FROM products p
                        LEFT JOIN product_categories pc ON p.category_id = pc.category_id
                        LEFT JOIN product_brands pb ON p.brand_id = pb.brand_id
                        ORDER BY p.product_id DESC"; // (แก้) 👈 (เราจะแสดงทั้งหมด ให้ Admin เห็น)

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allProducts.Add(new ProductDisplayModel
                            {
                                ProductId = reader.GetInt32("product_id"),
                                Name = reader.GetString("name"),
                                Price = reader.GetDecimal("price"),
                                StockQuantity = reader.GetInt32("stock_quantity"),
                                ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? null : reader.GetString("image_path"),
                                CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? (int?)null : reader.GetInt32("category_id"),
                                CategoryName = reader.IsDBNull(reader.GetOrdinal("category_name")) ? "ไม่มีหมวดหมู่" : reader.GetString("category_name"),
                                BrandId = reader.IsDBNull(reader.GetOrdinal("brand_id")) ? (int?)null : reader.GetInt32("brand_id"),
                                BrandName = reader.IsDBNull(reader.GetOrdinal("brand_name")) ? "ไม่มีแบรนด์" : reader.GetString("brand_name"),
                                IsActive = reader.GetBoolean("is_active")
                            });
                        }
                    }
                }
                ApplyFilter();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลสินค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 2. การค้นหาและกรองข้อมูล (เหมือนเดิม) ---
        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string searchText = txtSearch.Text.Trim().ToLower();
            filteredProducts.Clear();
            IEnumerable<ProductDisplayModel> query = allProducts;
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                query = allProducts.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    p.CategoryName.ToLower().Contains(searchText) ||
                    p.BrandName.ToLower().Contains(searchText)
                );
            }
            foreach (var item in query)
            {
                filteredProducts.Add(item);
            }
        }

        // --- 3. ปุ่ม เพิ่มสินค้าใหม่ (เหมือนเดิม) ---
        private void btnAddProduct_Click(object sender, RoutedEventArgs e)
        {
            AddEditProductWindow addEditWindow = new AddEditProductWindow();
            if (addEditWindow.ShowDialog() == true)
            {
                LoadProducts();
            }
        }

        // --- 4. ปุ่ม แก้ไขสินค้า (เหมือนเดิม) ---
        private void btnEditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                AddEditProductWindow addEditWindow = new AddEditProductWindow(productId);
                if (addEditWindow.ShowDialog() == true)
                {
                    LoadProducts();
                }
            }
        }

        // (เพิ่ม) 👈 (สำคัญ) เมธอดใหม่สำหรับปุ่ม "ซ่อน/แสดง"
        private void btnToggleActive_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductDisplayModel product)
            {
                bool newStatus = !product.IsActive; // (กลับค่า True -> False)
                string actionText = newStatus ? "แสดง" : "ซ่อน";

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "UPDATE products SET is_active = @status WHERE product_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@status", newStatus);
                            cmd.Parameters.AddWithValue("@id", product.ProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    CustomMessageBoxWindow.Show($"'ซ่อน/แสดง' สินค้าสำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    LoadProducts(); // (รีเฟรชตาราง)
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show($"เกิดข้อผิดพลาดในการ {actionText} สินค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // (แก้) 👈 (สำคัญ) เมธอดนี้จะกลายเป็น "ลบถาวร"
        private void btnDeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int productId)
            {
                MessageBoxResult result = MessageBox.Show(
                    "!! คำเตือน !!\nคุณแน่ใจหรือไม่ว่าต้องการ 'ลบถาวร' สินค้า ID: " + productId + "?\n\nการลบนี้จะลบข้อมูลสินค้า, ไซส์, รูปภาพแกลเลอรี, และข้อมูลใน Wishlist ทั้งหมดที่เกี่ยวข้อง\n(ข้อมูลในบิลเก่าจะยังอยู่ แต่จะหาชื่อสินค้าไม่เจอ)\n\n!!! การกระทำนี้ไม่สามารถย้อนกลับได้ !!!",
                    "ยืนยันการลบถาวร",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Error); // (เปลี่ยนเป็น Icon Error)

                if (result == MessageBoxResult.No) return;

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        // (Foreign Key ถูกตั้งเป็น ON DELETE CASCADE ในตาราง variants, images, wishlist)
                        string sql = "DELETE FROM products WHERE product_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", productId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    CustomMessageBoxWindow.Show("ลบสินค้าถาวรสำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    LoadProducts(); // (รีเฟรชตาราง)
                }
                catch (MySqlException ex) when (ex.Number == 1451) // (ดัก Error Foreign Key)
                {
                    CustomMessageBoxWindow.Show(
                        "ไม่สามารถลบสินค้าถาวรได้ (Error 1451)\n\nสาเหตุ: สินค้านี้ถูก 'สั่งซื้อ' ไปแล้ว (มีข้อมูลค้างอยู่ในตาราง 'order_items')\n\nคำแนะนำ: กรุณาใช้ปุ่ม 'ซ่อน' (Soft Delete) แทนการลบถาวร",
                        "Database Error",
                        CustomMessageBoxWindow.MessageBoxType.Error);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบถาวร: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }
    }
}