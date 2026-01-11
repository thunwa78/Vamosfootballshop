using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace login_store
{
    public partial class AddEditModelWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int modelId = 0;

        // Model สำหรับ ComboBox แบรนด์
        public class BrandItem
        {
            public int BrandId { get; set; }
            public string Name { get; set; }
        }

        public AddEditModelWindow(int id = 0)
        {
            InitializeComponent();
            this.modelId = id;
            if (this.modelId > 0)
            {
                txtTitle.Text = "แก้ไขรุ่นสินค้า";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBrands();
            if (this.modelId > 0)
            {
                LoadModelData();
            }
        }

        private void LoadBrands()
        {
            List<BrandItem> brands = new List<BrandItem>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT brand_id, name FROM product_brands ORDER BY name";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            brands.Add(new BrandItem
                            {
                                BrandId = reader.GetInt32("brand_id"),
                                Name = reader.GetString("name")
                            });
                        }
                    }
                }
                cmbBrand.ItemsSource = brands;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading brands: " + ex.Message);
            }
        }

        private void LoadModelData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT name, brand_id FROM product_models WHERE model_id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.modelId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtModelName.Text = reader.GetString("name");
                                cmbBrand.SelectedValue = reader.GetInt32("brand_id");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading model data: " + ex.Message);
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtModelName.Text) || cmbBrand.SelectedValue == null)
            {
                MessageBox.Show("กรุณากรอกชื่อรุ่นและเลือกแบรนด์", "ข้อมูลไม่ครบ");
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;
                    if (this.modelId == 0)
                    {
                        sql = "INSERT INTO product_models (name, brand_id) VALUES (@name, @brandId)";
                    }
                    else
                    {
                        sql = "UPDATE product_models SET name = @name, brand_id = @brandId WHERE model_id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", txtModelName.Text.Trim());
                        cmd.Parameters.AddWithValue("@brandId", cmbBrand.SelectedValue);
                        cmd.Parameters.AddWithValue("@id", this.modelId);
                        cmd.ExecuteNonQuery();
                    }
                }
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving data: " + ex.Message);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}