//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media; // 👈 (เพิ่ม) นี่คือตัวแก้ Error 'Brushes'

namespace login_store
{
    public partial class AdminManageCategoriesPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private ObservableCollection<ProductCategory> allCategories = new ObservableCollection<ProductCategory>();

        // (เพิ่ม) 1. ตัวแปรสำหรับเก็บว่ากำลังแก้ไข Category ตัวไหน
        private ProductCategory selectedCategoryForEdit = null;

        public AdminManageCategoriesPage()
        {
            InitializeComponent();
            CategoryListBox.ItemsSource = allCategories;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCategories();
        }

        // --- 1. โหลดหมวดหมู่ (Categories) ---
        private void LoadCategories()
        {
            allCategories.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT category_id, name FROM product_categories ORDER BY name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allCategories.Add(new ProductCategory
                            {
                                CategoryId = reader.GetInt32("category_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลหมวดหมู่: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) --- 2. บันทึกหมวดหมู่ (เพิ่ม หรือ แก้ไข) ---
        private void btnSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            string categoryName = txtCategoryName.Text.Trim();
            if (string.IsNullOrWhiteSpace(categoryName)) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "";

                    if (selectedCategoryForEdit == null)
                    {
                        // โหมด "เพิ่มใหม่" (INSERT)
                        sql = "INSERT INTO product_categories (name) VALUES (@name)";
                    }
                    else
                    {
                        // โหมด "แก้ไข" (UPDATE)
                        sql = "UPDATE product_categories SET name = @name WHERE category_id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", categoryName);

                        if (selectedCategoryForEdit != null)
                        {
                            // ถ้าเป็นโหมดแก้ไข ต้องใส่ @id ด้วย
                            cmd.Parameters.AddWithValue("@id", selectedCategoryForEdit.CategoryId);
                        }

                        cmd.ExecuteNonQuery();
                    }
                }

                // (แก้) 3. โหลดข้อมูลใหม่ และ Reset Form
                LoadCategories();
                ResetForm();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                CustomMessageBoxWindow.Show("ชื่อหมวดหมู่นี้มีอยู่แล้ว", "Duplicate Entry", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึก: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 3. ลบหมวดหมู่ (Categories) ---
        private void btnDeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            // (กันไว้) ถ้ากำลังแก้ไขอยู่ ให้ยกเลิกก่อน
            if (selectedCategoryForEdit != null)
            {
                CustomMessageBoxWindow.Show("กรุณากด 'ยกเลิก' การแก้ไขก่อนลบ", "Warning", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            if (sender is Button button && button.Tag is int categoryId)
            {
                MessageBoxResult confirm = MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบหมวดหมู่นี้?\n(สินค้าที่ใช้หมวดหมู่นี้อยู่จะถูกตั้งค่าเป็น 'ไม่มีหมวดหมู่')", "ยืนยันการลบ", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (confirm == MessageBoxResult.No) return;

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();

                        string sqlUpdate = "UPDATE products SET category_id = NULL WHERE category_id = @id";
                        using (MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdate, conn))
                        {
                            cmdUpdate.Parameters.AddWithValue("@id", categoryId);
                            cmdUpdate.ExecuteNonQuery();
                        }

                        string sqlDelete = "DELETE FROM product_categories WHERE category_id = @id";
                        using (MySqlCommand cmdDelete = new MySqlCommand(sqlDelete, conn))
                        {
                            cmdDelete.Parameters.AddWithValue("@id", categoryId);
                            cmdDelete.ExecuteNonQuery();
                        }
                    }
                    LoadCategories(); // โหลดใหม่
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบหมวดหมู่: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // (เพิ่ม) --- 4. ปุ่มกดแก้ไข ---
        private void btnEditCategory_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is ProductCategory categoryToEdit)
            {
                // เก็บ Category ที่เลือกไว้
                selectedCategoryForEdit = categoryToEdit;

                // เอาชื่อมาใส่ใน TextBox
                txtCategoryName.Text = categoryToEdit.Name;

                // เปลี่ยนปุ่ม "เพิ่ม" เป็น "บันทึกการแก้ไข"
                btnSaveCategory.Content = "บันทึกการแก้ไข";
                btnSaveCategory.Background = Brushes.OrangeRed; // (เปลี่ยนสีปุ่ม)

                // แสดงปุ่ม "ยกเลิก"
                btnCancelEdit.Visibility = Visibility.Visible;
            }
        }

        // (เพิ่ม) --- 5. ปุ่มยกเลิกการแก้ไข ---
        private void btnCancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ResetForm();
        }

        // (เพิ่ม) --- 6. เมธอดสำหรับ Reset ฟอร์ม ---
        private void ResetForm()
        {
            selectedCategoryForEdit = null; // ล้างตัวแปร
            txtCategoryName.Text = ""; // ล้าง TextBox

            // เปลี่ยนปุ่มกลับเป็นโหมด "เพิ่ม"
            btnSaveCategory.Content = "เพิ่มหมวดหมู่ใหม่";
            btnSaveCategory.Background = Brushes.DarkGreen; // (คืนสีปุ่ม)

            // ซ่อนปุ่ม "ยกเลิก"
            btnCancelEdit.Visibility = Visibility.Collapsed;
        }
    }
}