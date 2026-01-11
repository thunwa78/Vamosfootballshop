//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace login_store
{
    public partial class AddEditAdWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int editAdId = 0;

        // (แก้) 1. เราจะใช้ตัวแปรเดียวเก็บ Path ที่จะลง DB (แบบเดียวกับหน้า Product)
        private string currentImagePath = null;

        public AddEditAdWindow()
        {
            InitializeComponent();
            txtTitle.Text = "เพิ่มโฆษณาใหม่";
            this.editAdId = 0;
            LoadImagePreviewUI(null); // (แก้) เรียกใช้เมธอดใหม่
        }

        public AddEditAdWindow(int adId)
        {
            InitializeComponent();
            txtTitle.Text = "แก้ไขโฆษณา";
            this.editAdId = adId;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (editAdId > 0)
            {
                LoadAdData();
            }
            else
            {
                LoadImagePreviewUI(null);
            }
        }

        // (แก้) 2. "ตัวช่วย" โหลดรูปภาพจาก Relative Path (แบบเดียวกับ ShopPage)
        private BitmapImage GetBitmapImageFromRelativePath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;
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
            catch (Exception) { /* ถ้าโหลดไม่สำเร็จ ก็แค่ไม่แสดงรูป */ }
            return null;
        }

        // โหลดข้อมูลโฆษณา (กรณีแก้ไข)
        private void LoadAdData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT image_path, target_url, sort_order, is_active FROM ad_slides WHERE ad_id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.editAdId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // (แก้) 3. เก็บ Path ที่จะลง DB ไว้
                                currentImagePath = reader.GetString("image_path");
                                txtTargetUrl.Text = reader.GetString("target_url");
                                txtSortOrder.Text = reader.GetInt32("sort_order").ToString();
                                chkIsActive.IsChecked = reader.GetBoolean("is_active");

                                // (แก้) 4. โหลดพรีวิวจาก Relative Path
                                LoadImagePreviewUI(currentImagePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลโฆษณา: " + ex.Message, "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                this.Close();
            }
        }

        // (แก้) 5. "ตัวช่วย" คัดลอกไฟล์ (แบบเดียวกับหน้า Product)
        private (string OriginalPath, string RelativePath) BrowseAndCopyImage(string subFolder)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = openDialog.FileName;
                    // (แก้) 6. ใช้ GUID สร้างชื่อไฟล์ใหม่ ป้องกันไฟล์ซ้ำ
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFilePath);

                    string destinationFolder = Path.Combine(AppContext.BaseDirectory, "Images", subFolder);
                    Directory.CreateDirectory(destinationFolder);
                    string destinationFilePath = Path.Combine(destinationFolder, uniqueFileName);

                    File.Copy(sourceFilePath, destinationFilePath, true);

                    string relativePath = $"/Images/{subFolder}/{uniqueFileName}";

                    return (sourceFilePath, relativePath); // คืนค่า 2 อย่าง
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการคัดลอกไฟล์รูปภาพ: " + ex.Message, "File Error");
                    return (null, null);
                }
            }
            return (null, null);
        }


        // (แก้) 7. แก้ไขปุ่ม Browse
        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            var (originalPath, relativePath) = BrowseAndCopyImage("Ads"); // 👈 (ระบุโฟลเดอร์ Ads)

            if (relativePath != null)
            {
                // 1. บันทึก Relative Path (สำหรับ Database)
                currentImagePath = relativePath;

                // 2. (เพิ่ม) สร้าง BitmapImage จาก Original Path (สำหรับพรีวิว)
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(originalPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // ป้องกันไฟล์ล็อก
                bitmap.EndInit();

                // 3. แสดงพรีวิว
                imgPreview.Source = bitmap;
                txtImagePlaceholder.Visibility = Visibility.Collapsed;
            }
        }

        // (แก้) 8. แทนที่ LoadImagePreview(string path) เดิม
        private void LoadImagePreviewUI(string relativePath)
        {
            BitmapImage bitmap = GetBitmapImageFromRelativePath(relativePath);
            if (bitmap != null)
            {
                imgPreview.Source = bitmap;
                txtImagePlaceholder.Visibility = Visibility.Collapsed;
            }
            else
            {
                imgPreview.Source = null;
                txtImagePlaceholder.Visibility = Visibility.Visible;
                txtImagePlaceholder.Text = "No Image Selected";
            }
        }

        // (แก้) 9. แก้ไขปุ่มบันทึก (ง่ายขึ้นมาก)
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // (แก้) 10. เปลี่ยนไปเช็ค currentImagePath
            if (string.IsNullOrWhiteSpace(currentImagePath))
            {
                CustomMessageBoxWindow.Show("กรุณาเลือกรูปภาพโฆษณา", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            if (!int.TryParse(txtSortOrder.Text, out int sortOrder))
            {
                CustomMessageBoxWindow.Show("ลำดับการแสดงผลต้องเป็นตัวเลข", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            // (แก้) 11. ไม่ต้องมี Logic คัดลอกไฟล์ตรงนี้แล้ว
            // เพราะไฟล์ถูกคัดลอกไปแล้วตอนกด Browse

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;

                    if (editAdId == 0)
                    {
                        sql = "INSERT INTO ad_slides (image_path, target_url, sort_order, is_active) VALUES (@path, @url, @sort, @active)";
                    }
                    else
                    {
                        sql = "UPDATE ad_slides SET image_path = @path, target_url = @url, sort_order = @sort, is_active = @active WHERE ad_id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        // (แก้) 12. ใช้ currentImagePath ที่เราเก็บไว้
                        cmd.Parameters.AddWithValue("@path", currentImagePath);
                        cmd.Parameters.AddWithValue("@url", txtTargetUrl.Text.Trim());
                        cmd.Parameters.AddWithValue("@sort", sortOrder);
                        cmd.Parameters.AddWithValue("@active", chkIsActive.IsChecked);
                        if (editAdId > 0) cmd.Parameters.AddWithValue("@id", editAdId);

                        cmd.ExecuteNonQuery();
                    }
                }

                CustomMessageBoxWindow.Show("บันทึกโฆษณาสำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึกโฆษณา: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (ลบ) 13. เมธอด ConvertRelativePathToAbsolute ไม่จำเป็นต้องใช้อีกต่อไป

        // ตรวจสอบเฉพาะตัวเลขใน Sort Order
        private void Number_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}