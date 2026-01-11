//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace login_store
{
    // (Model สำหรับ Brand)
    public class ProductBrand
    {
        public int BrandId { get; set; }
        public string Name { get; set; }
    }

    public partial class AdminManageBrandsPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private List<ProductBrand> allBrands = new List<ProductBrand>();

        private int editingBrandId = 0;

        public AdminManageBrandsPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBrands();
        }

        // --- 1. โหลดข้อมูล ---
        private void LoadBrands()
        {
            allBrands.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT brand_id, name FROM product_brands ORDER BY name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allBrands.Add(new ProductBrand
                            {
                                BrandId = reader.GetInt32("brand_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
                BrandsDataGrid.ItemsSource = null;
                BrandsDataGrid.ItemsSource = allBrands;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลแบรนด์: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 2. บันทึก (เพิ่ม หรือ แก้ไข) ---
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            string brandName = txtBrandName.Text.Trim();
            if (string.IsNullOrWhiteSpace(brandName))
            {
                CustomMessageBoxWindow.Show("กรุณากรอกชื่อแบรนด์", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;
                    if (editingBrandId == 0) // ADD MODE
                    {
                        sql = "INSERT INTO product_brands (name) VALUES (@name)";
                    }
                    else // EDIT MODE
                    {
                        sql = "UPDATE product_brands SET name = @name WHERE brand_id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", brandName);
                        if (editingBrandId != 0)
                        {
                            cmd.Parameters.AddWithValue("@id", editingBrandId);
                        }
                        cmd.ExecuteNonQuery();
                    }
                }
                CustomMessageBoxWindow.Show("บันทึกข้อมูลแบรนด์สำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                ResetForm();
                LoadBrands();
            }
            catch (MySqlException ex) when (ex.Number == 1062) // Error: Duplicate entry
            {
                CustomMessageBoxWindow.Show("ชื่อแบรนด์นี้มีอยู่แล้วในระบบ", "Duplicate Entry", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึก: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 3. แก้ไข (Edit) ---
        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductBrand brandToEdit)
            {
                txtBrandName.Text = brandToEdit.Name;
                editingBrandId = brandToEdit.BrandId;
                btnSave.Content = "อัปเดต";
                btnCancel.Visibility = Visibility.Visible;
            }
        }

        // --- 4. ลบ (Delete) ---
        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int brandId)
            {
                MessageBoxResult result = MessageBox.Show(
                   "การลบแบรนด์จะทำให้สินค้าที่ใช้แบรนด์นี้ถูกตั้งค่าเป็น 'ไม่มีแบรนด์' (NULL)\nคุณต้องการดำเนินการต่อหรือไม่?",
                   "ยืนยันการลบ",
                   MessageBoxButton.YesNo,
                   MessageBoxImage.Warning);

                if (result == MessageBoxResult.No) return;

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        // (Foreign Key ใน products ถูกตั้งค่าเป็น ON DELETE SET NULL)
                        string sql = "DELETE FROM product_brands WHERE brand_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", brandId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    CustomMessageBoxWindow.Show("ลบแบรนด์สำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    ResetForm();
                    LoadBrands();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบ: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- 5. ยกเลิก (Cancel Edit) ---
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        private void ResetForm()
        {
            txtBrandName.Text = "";
            editingBrandId = 0;
            btnSave.Content = "บันทึก";
            btnCancel.Visibility = Visibility.Collapsed;
        }
    }
}