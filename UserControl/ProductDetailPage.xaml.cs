using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System.Linq;
using System.Text.RegularExpressions;
using System.IO; // 👈 (สำคัญ) ต้องใช้ File และ Path

namespace login_store
{
    public partial class ProductDetailPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentProductId;
        private int currentUserId;
        private List<ProductVariant> variantsList;
        private List<ProductImage> imageList; // 👈 (นี่คือคลาสจากไฟล์ ProductImage.cs)
        private string mainImagePath;
        private string sizeChartPath = null;
        private decimal currentPrice;
        private string currentProductName;

        public ProductDetailPage(SlideManage parent, int productId)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentProductId = productId;
            this.currentUserId = UserSession.UserId;
            this.variantsList = new List<ProductVariant>();
            this.imageList = new List<ProductImage>();
            this.DataContext = this;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadProductDetails();
        }

        // --------------------------------------------------------------------
        // (เพิ่ม) "ตัวช่วย" โหลดรูปภาพ
        // --------------------------------------------------------------------
        private BitmapImage LoadImagePreview(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return null;

            try
            {
                // 1. หา Path เต็มของไฟล์รูป (ที่อยู่ใน bin/Debug)
                string baseDir = AppContext.BaseDirectory;
                string path = relativePath.TrimStart('/');
                string fullPath = Path.Combine(baseDir, path);

                // 2. ตรวจสอบว่าไฟล์มีอยู่จริง
                if (File.Exists(fullPath))
                {
                    // 3. โหลดไฟล์เป็น BitmapImage
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // ป้องกันไฟล์ล็อก
                    bitmap.EndInit();
                    return bitmap;
                }
            }
            catch (Exception) { /* ถ้าโหลดไม่สำเร็จ ก็แค่ไม่แสดงรูป */ }

            return null; // คืนค่าว่างถ้าไม่เจอไฟล์
        }
        // --------------------------------------------------------------------


        private void LoadProductDetails()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlProduct = "SELECT p.name, p.price, p.description, p.image_path, p.size_chart_path, " +
                                        "b.name AS BrandName, c.name AS CategoryName " +
                                        "FROM products p " +
                                        "LEFT JOIN product_brands b ON p.brand_id = b.brand_id " +
                                        "LEFT JOIN product_categories c ON p.category_id = c.category_id " +
                                        "WHERE p.product_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sqlProduct, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.currentProductId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentProductName = reader.GetString("name");
                                currentPrice = reader.GetDecimal("price");
                                mainImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? null : reader.GetString("image_path");
                                sizeChartPath = reader.IsDBNull(reader.GetOrdinal("size_chart_path")) ? null : reader.GetString("size_chart_path");

                                string brand = reader.IsDBNull(reader.GetOrdinal("BrandName")) ? "" : reader.GetString("BrandName");
                                string category = reader.IsDBNull(reader.GetOrdinal("CategoryName")) ? "" : reader.GetString("CategoryName");

                                txtName.Text = currentProductName;
                                txtPrice.Text = $"{currentPrice:N2} บาท";
                                txtDescription.Text = reader.GetString("description");
                                txtBrandCategory.Text = $"{brand} / {category}";

                                // (แก้) ใช้ "ตัวช่วย" โหลดรูปปก
                                imgMainProduct.Source = LoadImagePreview(mainImagePath);
                            }
                            else
                            {
                                CustomMessageBoxWindow.Show("ไม่พบสินค้า", "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                                BackToShop_Click(null, null);
                                return;
                            }
                        }
                    }

                    variantsList.Clear();
                    string sqlVariants = "SELECT variant_id, size_name, stock_quantity FROM product_variants WHERE product_id = @id ORDER BY sort_order ASC, size_name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sqlVariants, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.currentProductId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                variantsList.Add(new ProductVariant
                                {
                                    VariantId = reader.GetInt32("variant_id"),
                                    SizeName = reader.GetString("size_name"),
                                    StockQuantity = reader.GetInt32("stock_quantity")
                                });
                            }
                        }
                    }
                } // ปิด Connection

                sizeListBox.ItemsSource = variantsList;
                if (variantsList.Count == 0)
                {
                    txtStock.Text = "สินค้าหมดสต็อก";
                    txtStock.Foreground = System.Windows.Media.Brushes.Red;
                    btnAddToCart.IsEnabled = false;
                }

                CheckIfFavorite();
                LoadProductImages(); // 👈 (แก้) เรียกใช้เมธอดที่แก้ไขแล้ว
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดรายละเอียดสินค้าได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) เมธอดนี้ถูกแก้ไขทั้งหมด
        private void LoadProductImages()
        {
            imageList.Clear();

            // 1. เพิ่ม "รูปปก" เป็นรูปแรกในแกลเลอรี
            if (!string.IsNullOrEmpty(mainImagePath))
            {
                imageList.Add(new ProductImage
                {
                    ImagePath = mainImagePath,
                    ImagePreview = LoadImagePreview(mainImagePath) // 👈 โหลดพรีวิว
                });
            }

            // 2. (เพิ่ม) ดึงรูปย่อยจากตาราง product_images
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlGallery = "SELECT image_path FROM product_images WHERE product_id = @id ORDER BY sort_order";
                    using (MySqlCommand cmd = new MySqlCommand(sqlGallery, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.currentProductId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string path = reader.GetString("image_path");
                                if (!string.IsNullOrEmpty(path))
                                {
                                    // เพิ่มรูปแกลเลอรี และ โหลดพรีวิว
                                    imageList.Add(new ProductImage
                                    {
                                        ImagePath = path,
                                        ImagePreview = LoadImagePreview(path) // 👈 โหลดพรีวิว
                                    });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดแกลเลอรีรูปภาพได้: " + ex.Message, "Database Error");
            }

            // 3. โหลด ListBox
            thumbnailListBox.ItemsSource = null;
            thumbnailListBox.ItemsSource = imageList;
            if (imageList.Count > 0)
            {
                thumbnailListBox.SelectedIndex = 0; // เลือกรูปแรก (รูปปก)
            }
        }

        // (แก้) Event เมื่อคลิก Thumbnail
        private void ThumbnailListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (thumbnailListBox.SelectedItem is ProductImage selectedImage)
            {
                // (แก้) ดึงรูปพรีวิวที่โหลดไว้แล้วมาแสดงเลย (ไม่ต้องโหลดใหม่)
                imgMainProduct.Source = selectedImage.ImagePreview;
            }
        }


        // --- (โค้ดส่วนที่เหลือทั้งหมดของคุณเหมือนเดิม) ---
        // (SizeListBox_SelectionChanged, Quantity_PreviewTextInput, 
        //  btnDecreaseQuantity_Click, btnIncreaseQuantity_Click, 
        //  BtnAddToCart_Click, CheckIfFavorite, BtnToggleFavorite_Click,
        //  BackToShop_Click, BtnSizeChart_Click, ImageZoom_Click,
        //  Window Control Methods...)

        private void SizeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sizeListBox.SelectedItem is ProductVariant selectedVariant)
            {
                txtStock.Text = $"เหลือในสต็อก: {selectedVariant.StockQuantity} ชิ้น";
                if (selectedVariant.StockQuantity == 0)
                {
                    txtStock.Foreground = System.Windows.Media.Brushes.Red;
                    btnAddToCart.IsEnabled = false;
                }
                else
                {
                    txtStock.Foreground = System.Windows.Media.Brushes.LightGreen;
                    btnAddToCart.IsEnabled = true;
                }
                if (int.TryParse(txtQuantity.Text, out int qty) && qty > selectedVariant.StockQuantity)
                {
                    txtQuantity.Text = "1";
                }
            }
            else
            {
                txtStock.Text = "กรุณาเลือกขนาด";
                txtStock.Foreground = System.Windows.Media.Brushes.Red;
                btnAddToCart.IsEnabled = false;
            }
        }

        private void Quantity_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "[0-9]");
        }

        private void btnDecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtQuantity.Text, out int qty))
            {
                if (qty > 1)
                    txtQuantity.Text = (qty - 1).ToString();
            }
        }

        private void btnIncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sizeListBox.SelectedItem == null)
            {
                CustomMessageBoxWindow.Show("กรุณาเลือกขนาดก่อนเพิ่มจำนวน", "Warning", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            if (int.TryParse(txtQuantity.Text, out int qty) && (sizeListBox.SelectedItem is ProductVariant selectedVariant))
            {
                if (qty < selectedVariant.StockQuantity)
                    txtQuantity.Text = (qty + 1).ToString();
                else
                    CustomMessageBoxWindow.Show($"สินค้าในสต็อกมีเพียง {selectedVariant.StockQuantity} ชิ้น", "สต็อกไม่พอ", CustomMessageBoxWindow.MessageBoxType.Warning);
            }
        }

        private void BtnAddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (sizeListBox.SelectedItem == null)
            {
                CustomMessageBoxWindow.Show("กรุณาเลือกขนาดสินค้า", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity <= 0)
            {
                CustomMessageBoxWindow.Show("กรุณากรอกจำนวนที่ถูกต้อง", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            var selectedVariant = sizeListBox.SelectedItem as ProductVariant;

            if (quantity > selectedVariant.StockQuantity)
            {
                CustomMessageBoxWindow.Show($"สินค้าในสต็อกมีเพียง {selectedVariant.StockQuantity} ชิ้น", "สต็อกไม่พอ", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    string sqlCheckCart = "SELECT quantity FROM cart_items WHERE user_id = @user_id AND product_variant_id = @variant_id";
                    int quantityInCart = 0;

                    using (MySqlCommand cmdCheck = new MySqlCommand(sqlCheckCart, conn))
                    {
                        cmdCheck.Parameters.AddWithValue("@user_id", this.currentUserId);
                        cmdCheck.Parameters.AddWithValue("@variant_id", selectedVariant.VariantId);

                        object result = cmdCheck.ExecuteScalar();
                        if (result != null)
                        {
                            quantityInCart = Convert.ToInt32(result);
                        }
                    }

                    int totalRequired = quantityInCart + quantity;

                    if (totalRequired > selectedVariant.StockQuantity)
                    {
                        CustomMessageBoxWindow.Show($"สินค้าในตะกร้ามีอยู่แล้ว {quantityInCart} ชิ้น หากเพิ่มอีก {quantity} ชิ้น จะเกินสต็อก ({selectedVariant.StockQuantity} ชิ้น) กรุณาลดจำนวนลง", "สต็อกไม่พอ", CustomMessageBoxWindow.MessageBoxType.Warning);
                        return;
                    }

                    string sql = "INSERT INTO cart_items (user_id, product_variant_id, quantity) " +
                                 "VALUES (@user_id, @variant_id, @quantity) " +
                                 "ON DUPLICATE KEY UPDATE quantity = quantity + @quantity_update";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        cmd.Parameters.AddWithValue("@variant_id", selectedVariant.VariantId);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@quantity_update", quantity);
                        cmd.ExecuteNonQuery();
                    }
                }
                CustomMessageBoxWindow.Show($"เพิ่ม {quantity} ชิ้น ({selectedVariant.SizeName}) ลงตะกร้าแล้ว", "เพิ่มสำเร็จ", CustomMessageBoxWindow.MessageBoxType.Success);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการเพิ่มสินค้าลงตะกร้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void CheckIfFavorite()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM wishlist_items WHERE user_id = @user_id AND product_id = @product_id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        cmd.Parameters.AddWithValue("@product_id", this.currentProductId);
                        long count = (long)cmd.ExecuteScalar();
                        btnToggleFavorite.IsChecked = (count > 0);
                    }
                }
            }
            catch (Exception) { /* Handle silently */ }
        }

        private void BtnToggleFavorite_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    bool isCurrentlyFavorite = btnToggleFavorite.IsChecked == true;

                    if (isCurrentlyFavorite)
                    {
                        string sql = "INSERT INTO wishlist_items (user_id, product_id) VALUES (@user_id, @product_id)";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                            cmd.Parameters.AddWithValue("@product_id", this.currentProductId);
                            cmd.ExecuteNonQuery();
                        }
                        CustomMessageBoxWindow.Show($"{currentProductName} ถูกเพิ่มในรายการโปรดแล้ว", "Wishlist", CustomMessageBoxWindow.MessageBoxType.Success);
                    }
                    else
                    {
                        string sql = "DELETE FROM wishlist_items WHERE user_id = @user_id AND product_id = @product_id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                            cmd.Parameters.AddWithValue("@product_id", this.currentProductId);
                            cmd.ExecuteNonQuery();
                        }
                        CustomMessageBoxWindow.Show($"{currentProductName} ถูกนำออกจากรายการโปรดแล้ว", "Wishlist", CustomMessageBoxWindow.MessageBoxType.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการจัดการรายการโปรด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                CheckIfFavorite();
            }
        }

        private void BackToShop_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ShopPage(parent), true);
        }

        private void BtnSizeChart_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(sizeChartPath))
            {
                CustomMessageBoxWindow.Show("สินค้านี้ไม่มีตารางไซส์", "Info", CustomMessageBoxWindow.MessageBoxType.Info);
                return;
            }

            // (TODO: เราต้องสร้างหน้าต่าง 'ImageViewerWindow' ในขั้นตอนต่อไป)
            //CustomMessageBoxWindow.Show($"TODO: เปิดหน้าต่าง ImageViewerWindow\nPath: {sizeChartPath}", "Info");

            // (โค้ดจริงจะเป็นแบบนี้)
            ImageViewerWindow viewer = new ImageViewerWindow(sizeChartPath);
            viewer.Owner = Window.GetWindow(this); // (ตั้งค่าให้ Popup อยู่ข้างบน)
            viewer.ShowDialog();
        }

        private void ImageZoom_Click(object sender, MouseButtonEventArgs e)
        {
            CustomMessageBoxWindow.Show("เปิดรูปภาพขนาดใหญ่สำหรับซูม/ดูมุมต่างๆ (ยังไม่ได้สร้าง)", "รูปภาพสินค้า", CustomMessageBoxWindow.MessageBoxType.Info);
        }

        // --- Window Control Methods ---
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null) { window.WindowState = WindowState.Minimized; }
        }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
        {
            Window window = Window.GetWindow(this);
            if (window != null) { window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized; }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == sender)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }
    }
}