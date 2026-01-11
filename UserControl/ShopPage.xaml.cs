using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MySqlConnector;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Windows.Threading;
using System.IO;

namespace login_store
{
    // (เพิ่ม) Class นี้ช่วยให้ ComboBox ทำงานง่ายขึ้น (เก็บ ID และ ชื่อ คู่กัน)
    public class FilterItem
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public partial class ShopPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private List<Product> allProducts = new List<Product>();
        private int currentUserId;

        private DispatcherTimer adTimer;
        private List<Tuple<string, string>> adDetails = new List<Tuple<string, string>>();
        private int currentAdIndex = 0;

        // (แก้) เปลี่ยนจาก Dictionary เป็น List<FilterItem> เพื่อรองรับ ComboBox แบบใหม่
        private List<FilterItem> brandsList = new List<FilterItem>();
        private List<FilterItem> categoriesList = new List<FilterItem>();
        private List<FilterItem> modelsList = new List<FilterItem>(); // (เพิ่ม) รายการรุ่น

        private bool isFilterLoading = false;

        public ShopPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadFilterOptions();
            LoadAds();
            LoadProducts(null);
        }

        // (ตัวช่วยโหลดรูปภาพ)
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

        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex)) return string.Empty;
            return reader.GetString(colIndex);
        }

        // --- (แก้) โหลด Filter Options (ใช้ FilterItem) ---
        private void LoadFilterOptions()
        {
            isFilterLoading = true;
            brandsList.Clear();
            categoriesList.Clear();

            // เพิ่มตัวเลือก "ทั้งหมด"
            brandsList.Add(new FilterItem { Id = 0, Name = "ทุกแบรนด์" });
            categoriesList.Add(new FilterItem { Id = 0, Name = "ทุกหมวดหมู่" });

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // โหลดแบรนด์
                    string sqlBrands = "SELECT brand_id, name FROM product_brands ORDER BY name";
                    using (MySqlCommand cmd = new MySqlCommand(sqlBrands, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            brandsList.Add(new FilterItem { Id = reader.GetInt32("brand_id"), Name = reader.GetString("name") });
                    }

                    // โหลดหมวดหมู่
                    string sqlCategories = "SELECT category_id, name FROM product_categories ORDER BY name";
                    using (MySqlCommand cmd = new MySqlCommand(sqlCategories, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                            categoriesList.Add(new FilterItem { Id = reader.GetInt32("category_id"), Name = reader.GetString("name") });
                    }
                }

                // ผูกข้อมูลเข้า ComboBox
                cmbBrandFilter.ItemsSource = brandsList;
                cmbBrandFilter.DisplayMemberPath = "Name";
                cmbBrandFilter.SelectedValuePath = "Id";
                cmbBrandFilter.SelectedIndex = 0;

                cmbCategoryFilter.ItemsSource = categoriesList;
                cmbCategoryFilter.DisplayMemberPath = "Name";
                cmbCategoryFilter.SelectedValuePath = "Id";
                cmbCategoryFilter.SelectedIndex = 0;

                // สร้างปุ่ม Quick Filter (หมวดหมู่)
                CategoryQuickFilterPanel.Children.Clear();
                foreach (var category in categoriesList)
                {
                    if (category.Id == 0) continue;
                    Button btn = new Button
                    {
                        Content = category.Name,
                        Tag = category.Name,
                        Margin = new Thickness(10, 0, 10, 0),
                        Foreground = Brushes.White,
                        FontSize = 16,
                        Background = Brushes.Transparent,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand
                    };
                    btn.Click += CategoryQuickFilter_Click;
                    CategoryQuickFilterPanel.Children.Add(btn);
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลด Filter: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            isFilterLoading = false;
        }

        // (เพิ่ม) เมธอดโหลด "รุ่น" ตามแบรนด์
        private void LoadModelsByBrand(int brandId)
        {
            modelsList.Clear();
            modelsList.Add(new FilterItem { Id = 0, Name = "ทุกรุ่น" }); // Default

            if (brandId > 0)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT model_id, name FROM product_models WHERE brand_id = @brandId ORDER BY name";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@brandId", brandId);
                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    modelsList.Add(new FilterItem { Id = reader.GetInt32("model_id"), Name = reader.GetString("name") });
                                }
                            }
                        }
                    }
                }
                catch (Exception ex) { Console.WriteLine("Error loading models: " + ex.Message); }

                // (ถ้าคุณเพิ่ม cmbModelFilter ใน XAML แล้ว จะไม่ Error)
                if (this.FindName("cmbModelFilter") is ComboBox cmbModel)
                {
                    cmbModel.IsEnabled = true;
                    cmbModel.ItemsSource = null;
                    cmbModel.ItemsSource = modelsList;
                    cmbModel.DisplayMemberPath = "Name";
                    cmbModel.SelectedValuePath = "Id";

                    bool oldState = isFilterLoading;
                    isFilterLoading = true;
                    cmbModel.SelectedIndex = 0;
                    isFilterLoading = oldState;
                }
            }
            else
            {
                if (this.FindName("cmbModelFilter") is ComboBox cmbModel)
                {
                    cmbModel.ItemsSource = null;
                    cmbModel.IsEnabled = false;
                }
            }
        }

        // --- Logic Ad Slideshow (เหมือนเดิม) ---
        private void LoadAds()
        {
            adDetails.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT image_path, target_url FROM ad_slides WHERE is_active = 1 ORDER BY sort_order ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (MySqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string path = GetStringSafe(reader, "image_path");
                            string url = GetStringSafe(reader, "target_url");
                            adDetails.Add(Tuple.Create(path, url));
                        }
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine("Ad Load Error: " + ex.Message); }

            if (adDetails.Count > 0)
            {
                ShowAdAtIndex(0);
                if (adTimer == null)
                {
                    adTimer = new DispatcherTimer();
                    adTimer.Interval = TimeSpan.FromSeconds(5);
                    adTimer.Tick += AdTimer_Tick;
                }
                adTimer.Start();
            }
            else if (adTimer != null) { adTimer.Stop(); }
        }
        private void AdTimer_Tick(object sender, EventArgs e)
        {
            currentAdIndex = (currentAdIndex + 1) % adDetails.Count;
            ShowAdAtIndex(currentAdIndex);
        }
        private void ShowAdAtIndex(int index)
        {
            if (adDetails.Count == 0 || index < 0 || index >= adDetails.Count) return;
            currentAdIndex = index;
            try
            {
                imgAdSlideshow.Source = LoadImagePreview(adDetails[currentAdIndex].Item1);
            }
            catch (Exception ex) { Console.WriteLine($"Error loading ad image: {ex.Message}"); }
        }
        private void btnAdPrev_Click(object sender, RoutedEventArgs e)
        {
            if (adDetails.Count == 0) return;
            currentAdIndex = (currentAdIndex - 1 + adDetails.Count) % adDetails.Count;
            ShowAdAtIndex(currentAdIndex);
            adTimer.Stop();
            adTimer.Start();
        }
        private void btnAdNext_Click(object sender, RoutedEventArgs e)
        {
            if (adDetails.Count == 0) return;
            currentAdIndex = (currentAdIndex + 1) % adDetails.Count;
            ShowAdAtIndex(currentAdIndex);
            adTimer.Stop();
            adTimer.Start();
        }

        // --- Ad Click ---
        private void Ad_Click(object sender, MouseButtonEventArgs e)
        {
            if (currentAdIndex < 0 || currentAdIndex >= adDetails.Count) return;
            string target = adDetails[currentAdIndex].Item2;
            if (string.IsNullOrEmpty(target)) return;

            if (target.StartsWith("product_list:"))
            {
                string idString = target.Substring(13);
                List<int> idList = idString.Split(',')
                                           .Select(s => int.TryParse(s, out int id) ? id : 0)
                                           .Where(id => id > 0)
                                           .ToList();
                if (idList.Count > 0)
                {
                    isFilterLoading = true;
                    txtSearch.Text = "";
                    cmbBrandFilter.SelectedIndex = 0;
                    cmbCategoryFilter.SelectedIndex = 0;
                    // รีเซ็ตรุ่น
                    if (this.FindName("cmbModelFilter") is ComboBox cmbModel)
                    {
                        cmbModel.ItemsSource = null;
                        cmbModel.IsEnabled = false;
                    }
                    isFilterLoading = false;
                    LoadProducts(idList);
                }
            }
            else if (target.StartsWith("product_id:"))
            {
                if (int.TryParse(target.Substring(11), out int productId))
                {
                    OpenProductDetail(productId);
                }
            }
            else if (target.StartsWith("category:"))
            {
                string categoryName = target.Substring(9);
                var item = categoriesList.FirstOrDefault(c => c.Name == categoryName);
                if (item != null) cmbCategoryFilter.SelectedValue = item.Id;
            }
            else if (target.StartsWith("brand:"))
            {
                string brandName = target.Substring(6);
                var item = brandsList.FirstOrDefault(b => b.Name == brandName);
                if (item != null) cmbBrandFilter.SelectedValue = item.Id;
            }
            else if (target.StartsWith("voucher_id:"))
            {
                if (int.TryParse(target.Substring(11), out int voucherId))
                {
                    CollectVoucherFromAd(voucherId);
                }
            }
        }

        private void CollectVoucherFromAd(int voucherId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "INSERT INTO user_vouchers (user_id, voucher_id) VALUES (@userId, @voucherId)";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                        cmd.Parameters.AddWithValue("@voucherId", voucherId);
                        cmd.ExecuteNonQuery();
                    }
                }
                CustomMessageBoxWindow.Show("เก็บโค้ดส่วนลดสำเร็จ!\nคุณสามารถใช้โค้ดนี้ได้ในหน้าชำระเงิน", "เก็บโค้ดสำเร็จ", CustomMessageBoxWindow.MessageBoxType.Success);
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                CustomMessageBoxWindow.Show("คุณมีโค้ดนี้อยู่แล้ว", "Info", CustomMessageBoxWindow.MessageBoxType.Info);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการเก็บโค้ด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- Load Products (อัปเกรด: รองรับ model_id) ---
        private void LoadProducts(List<int> specificProductIds = null)
        {
            allProducts.Clear();
            ProductWrapPanel.Children.Clear();

            // 1. ดึง Wishlist
            HashSet<int> userWishlistIds = new HashSet<int>();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlWishlist = "SELECT product_id FROM wishlist_items WHERE user_id = @userId";
                    using (MySqlCommand cmdWishlist = new MySqlCommand(sqlWishlist, conn))
                    {
                        cmdWishlist.Parameters.AddWithValue("@userId", this.currentUserId);
                        using (MySqlDataReader reader = cmdWishlist.ExecuteReader())
                        {
                            while (reader.Read()) userWishlistIds.Add(reader.GetInt32("product_id"));
                        }
                    }
                }
            }
            catch (Exception) { /* handle silently */ }

            // 2. ดึง Products
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT p.product_id, p.name, p.price, p.stock_quantity, p.image_path, p.category_id " +
                                 "FROM products p " +
                                 "WHERE p.is_active = 1 ";

                    MySqlCommand cmd = new MySqlCommand();

                    if (specificProductIds != null && specificProductIds.Count > 0)
                    {
                        // โหมดลิสต์ ID
                        var idParams = new List<string>();
                        for (int i = 0; i < specificProductIds.Count; i++)
                        {
                            string paramName = $"@id{i}";
                            idParams.Add(paramName);
                            cmd.Parameters.AddWithValue(paramName, specificProductIds[i]);
                        }
                        sql += $"AND p.product_id IN ({string.Join(", ", idParams)}) ";
                    }
                    else
                    {
                        // โหมด Filter ปกติ
                        string searchTerm = txtSearch.Text.Trim();
                        int brandId = (cmbBrandFilter.SelectedValue is int bId) ? bId : 0;
                        int categoryId = (cmbCategoryFilter.SelectedValue is int cId) ? cId : 0;

                        // (เพิ่ม) อ่านค่ารุ่น
                        int modelId = 0;
                        if (this.FindName("cmbModelFilter") is ComboBox cmbModel && cmbModel.SelectedValue is int mId)
                        {
                            modelId = mId;
                        }

                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            sql += "AND p.name LIKE @search ";
                            cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                        }
                        if (brandId > 0)
                        {
                            sql += "AND p.brand_id = @brand_id ";
                            cmd.Parameters.AddWithValue("@brand_id", brandId);
                        }
                        if (categoryId > 0)
                        {
                            sql += "AND p.category_id = @category_id ";
                            cmd.Parameters.AddWithValue("@category_id", categoryId);
                        }

                        // (เพิ่ม) กรองตามรุ่น
                        if (modelId > 0)
                        {
                            sql += "AND p.model_id = @model_id ";
                            cmd.Parameters.AddWithValue("@model_id", modelId);
                        }
                    }

                    cmd.Connection = conn;
                    cmd.CommandText = sql;

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
                                ImagePath = GetStringSafe(reader, "image_path"),
                                CategoryId = reader.IsDBNull(reader.GetOrdinal("category_id")) ? 0 : reader.GetInt32("category_id")
                            };
                            product.ImagePreview = LoadImagePreview(product.ImagePath);
                            product.IsInWishlist = userWishlistIds.Contains(product.ProductId);
                            allProducts.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลสินค้าได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }

            // 4. จัดเรียง
            var sortedProducts = allProducts
                .OrderByDescending(p => p.StockQuantity > 0 ? 1 : 0)
                .ThenBy(p => p.CategoryId)
                .ThenBy(p => p.Name);

            // 5. สร้างการ์ด
            foreach (var product in sortedProducts)
            {
                ProductWrapPanel.Children.Add(CreateProductCard(product));
            }
        }

        // --- เมธอด CreateProductCard (เหมือนเดิม) ---
        private Border CreateProductCard(Product product)
        {
            Border cardBorder = new Border
            {
                Width = 240,
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#2C2C2E")),
                CornerRadius = new CornerRadius(8),
                Margin = new Thickness(10),
                Cursor = Cursors.Hand,
                Tag = product.ProductId
            };
            cardBorder.MouseLeftButtonUp += OpenProductDetail_Click;
            StackPanel cardContent = new StackPanel();
            Border imageBorder = new Border
            {
                Height = 220,
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Background = Brushes.Black,
                ClipToBounds = true
            };
            System.Windows.Controls.Image productImage = new System.Windows.Controls.Image
            {
                Stretch = Stretch.UniformToFill,
                Source = product.ImagePreview
            };
            imageBorder.Child = productImage;
            cardContent.Children.Add(imageBorder);
            StackPanel textPanel = new StackPanel
            {
                Margin = new Thickness(10, 10, 10, 10)
            };
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
            Grid bottomGrid = new Grid
            {
                Margin = new Thickness(0, 5, 0, 0)
            };
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            bottomGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            if (product.StockQuantity > 0)
            {
                TextBlock productPrice = new TextBlock
                {
                    Text = $"{product.Price:N2} บาท",
                    Foreground = (Brush)new BrushConverter().ConvertFromString("#8fe48f"),
                    FontWeight = FontWeights.Bold,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(productPrice, 0);
                bottomGrid.Children.Add(productPrice);

                Button wishlistButton = new Button
                {
                    Content = "❤️",
                    Tag = product.ProductId,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Style = (Style)this.FindResource("WishlistButtonStyle")
                };
                if (product.IsInWishlist) wishlistButton.Foreground = Brushes.Red;
                else wishlistButton.Foreground = (Brush)new BrushConverter().ConvertFromString("#333333");
                wishlistButton.Click += WishlistButton_Click;
                Grid.SetColumn(wishlistButton, 1);
                bottomGrid.Children.Add(wishlistButton);
            }
            else
            {
                TextBlock soldOutText = new TextBlock
                {
                    Text = "สินค้าหมด",
                    Foreground = Brushes.DarkGray,
                    FontWeight = FontWeights.Bold,
                    FontSize = 18,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(soldOutText, 0);
                Grid.SetColumnSpan(soldOutText, 2);
                bottomGrid.Children.Add(soldOutText);
                cardBorder.Opacity = 0.6;
            }
            textPanel.Children.Add(bottomGrid);
            cardContent.Children.Add(textPanel);
            cardBorder.Child = cardContent;
            return cardBorder;
        }

        // --- Event Handlers (Filters) ---
        private void FilterChanged_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (isFilterLoading) return;

            // (แก้) ถ้าแบรนด์เปลี่ยน -> โหลดรุ่นใหม่
            if (sender == cmbBrandFilter)
            {
                if (cmbBrandFilter.SelectedValue is int brandId)
                {
                    LoadModelsByBrand(brandId);
                }
            }
            LoadProducts(null);
        }

        // (เพิ่ม) 👈 Event เมื่อเลือก "รุ่น" เปลี่ยน
        private void ModelFilterChanged_Selection(object sender, SelectionChangedEventArgs e)
        {
            if (isFilterLoading) return;
            LoadProducts(null);
        }

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            LoadProducts(null);
        }

        private void CategoryQuickFilter_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string categoryName)
            {
                var item = categoriesList.FirstOrDefault(c => c.Name == categoryName);
                if (item != null) cmbCategoryFilter.SelectedValue = item.Id;
            }
        }

        // (แก้) ปุ่ม Refresh
        private void btnRefresh_Click(object sender, RoutedEventArgs e)
        {
            isFilterLoading = true;
            txtSearch.Text = "";
            cmbCategoryFilter.SelectedIndex = 0;
            cmbBrandFilter.SelectedIndex = 0;
            // (รีเซ็ตรุ่น)
            if (this.FindName("cmbModelFilter") is ComboBox cmbModel)
            {
                cmbModel.ItemsSource = null;
                cmbModel.IsEnabled = false;
            }
            isFilterLoading = false;
            LoadProducts(null);
        }

        // --- Event Handlers (Navigation) & Others (เหมือนเดิม) ---
        private void OpenProductDetail_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is int productId)
            {
                if (e.OriginalSource is Button) return;
                OpenProductDetail(productId);
            }
        }
        private void OpenProductDetail(int productId)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new ProductDetailPage(parent, productId), false);
        }
        private void WishlistButton_Click(object sender, RoutedEventArgs e)
        { /* ...โค้ดเดิม... */
            if (!(sender is Button button) || !(button.Tag is int productId)) return;
            Product product = allProducts.FirstOrDefault(p => p.ProductId == productId);
            if (product == null) return;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;
                    if (product.IsInWishlist)
                    {
                        sql = "DELETE FROM wishlist_items WHERE user_id = @userId AND product_id = @productId";
                    }
                    else
                    {
                        sql = "INSERT INTO wishlist_items (user_id, product_id) VALUES (@userId, @productId)";
                    }
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                        cmd.Parameters.AddWithValue("@productId", productId);
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0)
                        {
                            if (product.IsInWishlist)
                            {
                                product.IsInWishlist = false;
                                button.Foreground = (Brush)new BrushConverter().ConvertFromString("#333333");
                            }
                            else
                            {
                                product.IsInWishlist = true;
                                button.Foreground = Brushes.Red;
                                CustomMessageBoxWindow.Show("เพิ่มในรายการโปรดแล้ว", "สำเร็จ", CustomMessageBoxWindow.MessageBoxType.Success);
                            }
                        }
                    }
                }
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                CustomMessageBoxWindow.Show("คุณมีสินค้านี้ในรายการโปรดอยู่แล้ว", "Info", CustomMessageBoxWindow.MessageBoxType.Info);
                product.IsInWishlist = true;
                button.Foreground = Brushes.Red;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new MyVouchersPage(parent), false);
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e) { Window window = Window.GetWindow(this); if (window != null) window.WindowState = WindowState.Minimized; }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e) { Window window = Window.GetWindow(this); if (window != null) window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized; }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.OriginalSource.GetType() == typeof(Border)) Window.GetWindow(this)?.DragMove(); }

        private void btnCart_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new CartPage(parent), false); }
        private void btnOrders_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), false); }
        private void btnNotifications_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new NotificationsPage(parent), false); }
        private void btnWishlist_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), false); }
        private void btnProfile_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new UserProfilePage(parent), false); }
        private void LogoutButton_Click(object sender, RoutedEventArgs e) { UserSession.EndSession(); if (parent != null) parent.NavigateWithSlide(new LoginPage(parent), true); }

        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new AboutUsPage(parent), false);
        }
    }
}