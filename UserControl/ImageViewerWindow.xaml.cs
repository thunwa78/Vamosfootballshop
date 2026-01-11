using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace login_store
{
    public partial class ImageViewerWindow : Window
    {
        private string imagePathToLoad;

        // (Constructor: รับ Path รูปที่ส่งมาจาก ProductDetailPage)
        public ImageViewerWindow(string relativeImagePath)
        {
            InitializeComponent();
            this.imagePathToLoad = relativeImagePath;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadImage();
        }

        private void LoadImage()
        {
            if (string.IsNullOrEmpty(imagePathToLoad))
            {
                txtTitle.Text = "ไม่พบรูปภาพ";
                return;
            }

            try
            {
                // (ใช้ Logic เดียวกับ LoadImagePreview)
                string baseDir = AppContext.BaseDirectory;
                string path = imagePathToLoad.TrimStart('/');
                string fullPath = Path.Combine(baseDir, path);

                if (File.Exists(fullPath))
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    imgViewer.Source = bitmap;
                }
                else
                {
                    txtTitle.Text = "ไม่พบไฟล์รูปภาพ";
                }
            }
            catch (Exception ex)
            {
                txtTitle.Text = "Error";
                MessageBox.Show("เกิดข้อผิดพลาดในการโหลดรูปภาพ: " + ex.Message);
            }
        }

        // --- ปุ่มปิด / ลากหน้าต่าง ---
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}