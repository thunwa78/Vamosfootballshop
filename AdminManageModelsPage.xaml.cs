using MySqlConnector;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace login_store
{
    // Model สำหรับแสดงในตาราง (มีชื่อแบรนด์ด้วย)
    public class ModelDisplay
    {
        public int ModelId { get; set; }
        public string Name { get; set; }
        public int BrandId { get; set; }
        public string BrandName { get; set; }
    }

    public partial class AdminManageModelsPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private ObservableCollection<ModelDisplay> allModels = new ObservableCollection<ModelDisplay>();

        public AdminManageModelsPage()
        {
            InitializeComponent();
            ModelsDataGrid.ItemsSource = allModels;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadModels();
        }

        private void LoadModels()
        {
            allModels.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // JOIN เพื่อเอาชื่อ Brand มาแสดง
                    string sql = @"
                        SELECT m.model_id, m.name, m.brand_id, b.name AS brand_name
                        FROM product_models m
                        JOIN product_brands b ON m.brand_id = b.brand_id
                        ORDER BY b.name, m.name";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            allModels.Add(new ModelDisplay
                            {
                                ModelId = reader.GetInt32("model_id"),
                                Name = reader.GetString("name"),
                                BrandId = reader.GetInt32("brand_id"),
                                BrandName = reader.GetString("brand_name")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลรุ่นสินค้า: " + ex.Message, "Database Error");
            }
        }

        private void btnAddModel_Click(object sender, RoutedEventArgs e)
        {
            // (TODO: เดี๋ยวเราจะสร้าง AddEditModelWindow ในขั้นตอนถัดไป)
            AddEditModelWindow window = new AddEditModelWindow();
            if (window.ShowDialog() == true)
            {
                LoadModels();
            }
        }

        private void btnEditModel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int modelId)
            {
                AddEditModelWindow window = new AddEditModelWindow(modelId);
                if (window.ShowDialog() == true)
                {
                    LoadModels();
                }
            }
        }

        private void btnDeleteModel_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int modelId)
            {
                if (MessageBox.Show($"คุณแน่ใจหรือไม่ว่าต้องการลบรุ่น ID: {modelId}?", "ยืนยันการลบ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sql = "DELETE FROM product_models WHERE model_id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", modelId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadModels();
                        CustomMessageBoxWindow.Show("ลบรุ่นสินค้าสำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบ: " + ex.Message, "Database Error");
                    }
                }
            }
        }
    }
}