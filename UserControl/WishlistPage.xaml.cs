using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System.Collections.Generic;
using System;
using System.Linq;
using System.IO;

namespace login_store
{
    public partial class WishlistPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;

        public WishlistPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadWishlist();
        }

        // (ตัวช่วยโหลดรูปภาพ - เหมือนเดิม)
        private BitmapImage LoadImagePreview(string relativePath)
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
            catch (Exception) { /* Handle error */ }
            return null;
        }

        // (Helper Method: GetStringSafe - จำเป็น)
        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex))
            {
                return string.Empty;
            }
            return reader.GetString(colIndex);
        }

        // --- 1. โหลดสินค้าใน Wishlist ---
        private void LoadWishlist()
        {
            WishlistWrapPanel.Children.Clear();
            txtNoItems.Visibility = Visibility.Collapsed;
            List<Product> wishlistProducts = new List<Product>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT p.product_id, p.name, p.price, p.stock_quantity, p.image_path 
                        FROM wishlist_items w
                        JOIN products p ON w.product_id = p.product_id
                        WHERE w.user_id = @user_id AND p.is_active = 1";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                Product product = new Product
                                {
                                    ProductId = reader.GetInt32("product_id"),
                                    Name = GetStringSafe(reader, "name"),
                                    Price = reader.GetDecimal("price"),
                                    StockQuantity = reader.GetInt32("stock_quantity"),
                                    ImagePath = GetStringSafe(reader, "image_path")
                                };

                                product.ImagePreview = LoadImagePreview(product.ImagePath);

                                // (แก้) 👈 (สำคัญ) ตั้งค่าให้ปุ่มหัวใจเป็นสีแดง
                                product.IsInWishlist = true;

                                wishlistProducts.Add(product);
                            }
                        }
                    }
                }

                foreach (var product in wishlistProducts)
                {
                    WishlistWrapPanel.Children.Add(CreateProductCard(product));
                }

                if (wishlistProducts.Count == 0)
                {
                    txtNoItems.Visibility = Visibility.Visible;
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดรายการโปรด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) 👈 (สำคัญ) แทนที่ด้วย CreateProductCard (v2) จาก ShopPage
        private Border CreateProductCard(Product product)
        {
            // 1. การ์ดหลัก (ใหญ่ขึ้น, ขอบมน)
            Border cardBorder = new Border
            {
                Width = 240,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C2E")),
                CornerRadius = new CornerRadius(8), // <-- ขอบมนของการ์ด
                Margin = new Thickness(10),
                Cursor = Cursors.Hand,
                Tag = product.ProductId
            };
            cardBorder.MouseLeftButtonUp += OpenProductDetail_Click;

            // 2. ใช้ StackPanel (แนวตั้ง) เป็นตัวหลัก
            StackPanel cardContent = new StackPanel();

            // 3. สร้าง Border ครอบรูปภาพ (เพื่อให้ขอบมนและตัดขอบ)
            Border imageBorder = new Border
            {
                Height = 220, // <-- รูปใหญ่ขึ้น
                CornerRadius = new CornerRadius(8, 8, 0, 0), // <-- ขอบมนเฉพาะด้านบน
                Background = Brushes.Black, // พื้นหลังสำรองสีดำ
                ClipToBounds = true // <-- บังคับให้รูปถูกตัดตามขอบ
            };

            // 4. รูปภาพ (ไม่มี Margin)
            System.Windows.Controls.Image productImage = new System.Windows.Controls.Image // 👈 (ระบุชัดเจน)
            {
                Stretch = Stretch.UniformToFill,
                Source = product.ImagePreview
            };

            imageBorder.Child = productImage; // เอารูปใส่กรอบ
            cardContent.Children.Add(imageBorder); // เอากรอบใส่การ์ด

            // 5. StackPanel สำหรับ Text (ปรับ Margin)
            StackPanel textPanel = new StackPanel
            {
                Margin = new Thickness(10, 10, 10, 10)
            };

            // 6. ชื่อสินค้า (ปรับ Font และความสูง)
            TextBlock productName = new TextBlock
            {
                Text = product.Name,
                Foreground = Brushes.White,
                FontWeight = FontWeights.SemiBold,
                FontSize = 15,
                TextWrapping = TextWrapping.Wrap,
                Height = 36,
                Margin = new Thickness(0, 0, 0, 5)
            };
            textPanel.Children.Add(productName);

            // 7. (ใหม่) สร้าง Grid 2 คอลัมน์ (สำหรับ ราคา และ ปุ่มหัวใจ)
            Grid bottomGrid = new Grid
            {
                Margin = new Thickness(0, 5, 0, 0)
            };
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            // 8. ราคา (ย้ายเข้า Grid คอลัมน์ 0)
            TextBlock productPrice = new TextBlock
            {
                Text = $"{product.Price:N2} บาท",
                Foreground = (Brush)new BrushConverter().ConvertFromString("#8fe48f"),
                FontWeight = FontWeights.Bold,
                FontSize = 18,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = System.Windows.VerticalAlignment.Center // 👈 (ระบุชัดเจน)
            };
            Grid.SetColumn(productPrice, 0);
            bottomGrid.Children.Add(productPrice);

            // 9. ปุ่มหัวใจ (ย้ายเข้า Grid คอลัมน์ 1)
            Button wishlistButton = new Button
            {
                Content = "❤️",
                Tag = product.ProductId,
                HorizontalAlignment = HorizontalAlignment.Right,
                Style = (Style)this.FindResource("WishlistButtonStyle") // 👈 (ต้องมี Style นี้ใน XAML)
            };

            // ตั้งค่าสีปุ่มหัวใจตามสถานะ
            if (product.IsInWishlist)
            {
                wishlistButton.Foreground = Brushes.Red; // สีแดง
            }
            else
            {
                wishlistButton.Foreground = (Brush)new BrushConverter().ConvertFromString("#333333"); // สีเทาเข้ม
            }

            wishlistButton.Click += WishlistButton_Click; // 👈 (เรียกเมธอดใหม่)
            Grid.SetColumn(wishlistButton, 1);
            bottomGrid.Children.Add(wishlistButton);

            // 10. เพิ่ม Grid (ที่มืราคาและหัวใจ) เข้าไปใน Text Panel
            textPanel.Children.Add(bottomGrid);

            // 11. ประกอบร่าง
            cardContent.Children.Add(textPanel);
            cardBorder.Child = cardContent;

            return cardBorder;
        }

        // --- 3. Event Handlers ---

        // (เพิ่ม) 👈 (สำคัญ) เมธอดใหม่สำหรับปุ่มหัวใจ
        private void WishlistButton_Click(object sender, RoutedEventArgs e)
        {
            // (ในหน้านี้ เมื่อกดหัวใจ = ลบออก)
            if (!(sender is Button button) || !(button.Tag is int productId)) return;

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "DELETE FROM wishlist_items WHERE user_id = @userId AND product_id = @productId";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        int rows = cmd.ExecuteNonQuery();

                        if (rows > 0)
                        {
                            CustomMessageBoxWindow.Show("ลบออกจากรายการโปรดแล้ว", "สำเร็จ", CustomMessageBoxWindow.MessageBoxType.Success);
                            // (สำคัญ) โหลดหน้านี้ใหม่ เพื่อให้การ์ดหายไป
                            LoadWishlist();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (เมธอดนี้ยังคงใช้ เมื่อคลิกที่ "การ์ด" (Border)
        private void OpenProductDetail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int productId)
            {
                // (เช็คว่าถ้าเผลอคลิกปุ่มหัวใจ มันจะไม่ทำงาน)
                if (e.OriginalSource is Button) return;
                OpenProductDetail(productId);
            }
        }

        private void OpenProductDetail(int productId)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ProductDetailPage(parent, productId), false);
        }

        // --- 4. เมธอดควบคุมหน้าต่าง และ Top Nav Bar (เหมือนเดิม) ---

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
            if (e.Source.GetType() == typeof(Border) || e.Source.GetType() == typeof(Grid) || e.Source.GetType() == typeof(ScrollViewer))
            {
                Window.GetWindow(this)?.DragMove();
            }
        }
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            btnShop_Click(sender, e);
        }
        private void btnShop_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ShopPage(parent), true);
        }
        private void btnCart_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new CartPage(parent), false);
        }
        private void btnOrders_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), true);
        }
        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new MyVouchersPage(parent), false);
            }
        }
        private void btnNotifications_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new NotificationsPage(parent), true);
        }
        private void btnWishlist_Click(object sender, RoutedEventArgs e)
        {
            // (คุณอยู่ที่หน้านี้แล้ว ไม่ต้องทำอะไร)
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new UserProfilePage(parent), true);
        }

        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                // (เรียกใช้หน้าที่เราเพิ่งสร้าง)
                SlideManage.Instance.NavigateWithSlide(new AboutUsPage(parent), false);
            }
        }
    }
}