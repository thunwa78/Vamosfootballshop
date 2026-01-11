using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO; // 👈 (เพิ่ม) 1. ต้องใช้ File และ Path

namespace login_store
{
    // (คลาส Model ที่ใช้ในการโหลดรายการตะกร้า)
    public class CartItemDetails
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public int VariantId { get; set; }
        public string Name { get; set; }
        public string SizeName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImagePath { get; set; } // (สำหรับ DB)

        // (เพิ่ม) 👈 2. เพิ่มที่เก็บรูปพรีวิว
        public BitmapImage ImagePreview { get; set; }
    }

    public partial class CartPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;
        private decimal totalPrice = 0;

        public CartPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCartItems();
        }

        // (เพิ่ม) 👈 3. เพิ่ม "ตัวช่วย" โหลดรูปภาพ
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
            catch (Exception) { /* ถ้าโหลดไม่สำเร็จ ก็แค่ไม่แสดงรูป */ }
            return null;
        }

        // --- 1. โหลดรายการตะกร้า (หลัก) ---
        private void LoadCartItems()
        {
            CartItemsPanel.Children.Clear();
            totalPrice = 0;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT 
                            ci.cart_item_id, p.product_id, pv.variant_id AS product_variant_id,
                            p.name, pv.size_name, p.price, ci.quantity, p.image_path 
                        FROM cart_items ci
                        JOIN product_variants pv ON ci.product_variant_id = pv.variant_id
                        JOIN products p ON pv.product_id = p.product_id
                        WHERE ci.user_id = @user_id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CartItemDetails item = new CartItemDetails
                                {
                                    CartItemId = reader.GetInt32("cart_item_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    VariantId = reader.GetInt32("product_variant_id"),
                                    Name = reader.GetString("name"),
                                    SizeName = reader.GetString("size_name"),
                                    Price = reader.GetDecimal("price"),
                                    Quantity = reader.GetInt32("quantity"),
                                    ImagePath = reader.IsDBNull(reader.GetOrdinal("image_path")) ? null : reader.GetString("image_path")
                                };

                                // (เพิ่ม) 👈 4. เรียกใช้ตัวช่วยโหลดพรีวิว
                                item.ImagePreview = LoadImagePreview(item.ImagePath);

                                totalPrice += item.Price * item.Quantity;
                                CartItemsPanel.Children.Add(CreateCartItemCard(item));
                            }
                        }
                    }
                    txtTotalPrice.Text = $"{totalPrice:N2} บาท";

                    if (totalPrice == 0)
                    {
                        btnCheckout.IsEnabled = false;
                        btnCheckout.Content = "ตะกร้าว่างเปล่า";
                    }
                    else
                    {
                        btnCheckout.IsEnabled = true;
                        btnCheckout.Content = "ดำเนินการชำระเงิน";
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลตะกร้าได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 2. สร้าง Card สินค้า (พร้อมปุ่ม + / -) ---
        private Border CreateCartItemCard(CartItemDetails item)
        {
            Border cardBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(42, 255, 255, 255)),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(0, 0, 0, 10),
                Padding = new Thickness(10, 10, 10, 10)
            };

            Grid grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Name & Size
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Qty Stepper
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) }); // Price
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Delete

            // (แก้) 👈 5. แก้ไข Image Control นี้
            Image productImage = new Image
            {
                Width = 70,
                Height = 70,
                Stretch = Stretch.Uniform,
                Source = item.ImagePreview // 👈 (สำคัญ) ดึงจากพรีวิวที่โหลดไว้
            };
            Grid.SetColumn(productImage, 0);

            StackPanel namePanel = new StackPanel { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            namePanel.Children.Add(new TextBlock { Text = item.Name, Foreground = Brushes.White, FontSize = 16 });
            namePanel.Children.Add(new TextBlock { Text = $"ขนาด: {item.SizeName}", Foreground = Brushes.LightGray, FontSize = 12 });
            Grid.SetColumn(namePanel, 1);

            StackPanel qtyStepperPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetColumn(qtyStepperPanel, 2);

            Button btnDecrease = new Button { Content = "-", Style = (Style)this.Resources["QuantityButton"], Tag = item.VariantId };
            btnDecrease.Click += BtnDecreaseQuantity_Click;

            TextBlock txtQuantityDisplay = new TextBlock { Text = item.Quantity.ToString(), VerticalAlignment = VerticalAlignment.Center, Width = 30, TextAlignment = TextAlignment.Center, FontWeight = FontWeights.Bold, Foreground = Brushes.White };

            Button btnIncrease = new Button { Content = "+", Style = (Style)this.Resources["QuantityButton"], Tag = item.VariantId };
            btnIncrease.Click += BtnIncreaseQuantity_Click;

            qtyStepperPanel.Children.Add(btnDecrease);
            qtyStepperPanel.Children.Add(txtQuantityDisplay);
            qtyStepperPanel.Children.Add(btnIncrease);

            TextBlock subTotalPrice = new TextBlock { Text = $"{(item.Price * item.Quantity):N2} บาท", Foreground = Brushes.LightGreen, FontSize = 16, FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right };
            Grid.SetColumn(subTotalPrice, 3);

            Button removeButton = new Button { Content = "ลบ", Background = Brushes.DarkRed, Foreground = Brushes.White, Padding = new Thickness(10, 5, 10, 5), Margin = new Thickness(10, 0, 0, 0), Tag = item.CartItemId };
            removeButton.Click += RemoveButton_Click;
            Grid.SetColumn(removeButton, 4);

            grid.Children.Add(productImage);
            grid.Children.Add(namePanel);
            grid.Children.Add(qtyStepperPanel);
            grid.Children.Add(subTotalPrice);
            grid.Children.Add(removeButton);

            cardBorder.Child = grid;
            return cardBorder;
        }

        // --- 3. Logic การอัปเดตจำนวน (เหมือนเดิม) ---
        private void UpdateCartItemQuantity(int variantId, int newQuantity)
        {
            if (newQuantity <= 0)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM cart_items WHERE user_id = @user_id AND product_variant_id = @variant_id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                            cmd.Parameters.AddWithValue("@variant_id", variantId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                catch (Exception ex) { CustomMessageBoxWindow.Show("Error deleting item: " + ex.Message, "Error"); }
                LoadCartItems();
                return;
            }

            try
            {
                int maxStock = 0;
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sqlStock = "SELECT stock_quantity FROM product_variants WHERE variant_id = @variant_id";
                    using (MySqlCommand cmdStock = new MySqlCommand(sqlStock, conn))
                    {
                        cmdStock.Parameters.AddWithValue("@variant_id", variantId);
                        object result = cmdStock.ExecuteScalar();
                        if (result != null)
                        {
                            maxStock = Convert.ToInt32(result);
                        }
                    }

                    if (newQuantity > maxStock)
                    {
                        CustomMessageBoxWindow.Show($"ไม่สามารถเพิ่มจำนวนสินค้าได้ สินค้าในสต็อกมีเพียง {maxStock} ชิ้น", "สต็อกไม่พอ", CustomMessageBoxWindow.MessageBoxType.Warning);
                        return;
                    }

                    string sqlUpdate = "UPDATE cart_items SET quantity = @new_quantity WHERE user_id = @user_id AND product_variant_id = @variant_id";
                    using (MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdate, conn))
                    {
                        cmdUpdate.Parameters.AddWithValue("@new_quantity", newQuantity);
                        cmdUpdate.Parameters.AddWithValue("@user_id", this.currentUserId);
                        cmdUpdate.Parameters.AddWithValue("@variant_id", variantId);
                        cmdUpdate.ExecuteNonQuery();
                    }
                }
                LoadCartItems();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการอัปเดตจำนวนสินค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- (โค้ด Event Handlers ที่เหลือ - เหมือนเดิม) ---

        private void BtnIncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int variantId)
            {
                try
                {
                    int currentQuantity = 0;
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT quantity FROM cart_items WHERE user_id = @user_id AND product_variant_id = @variant_id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                            cmd.Parameters.AddWithValue("@variant_id", variantId);
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                currentQuantity = Convert.ToInt32(result);
                            }
                        }
                    }
                    UpdateCartItemQuantity(variantId, currentQuantity + 1);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการดึงจำนวนปัจจุบัน: " + ex.Message, "Error");
                }
            }
        }

        private void BtnDecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int variantId)
            {
                try
                {
                    int currentQuantity = 0;
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "SELECT quantity FROM cart_items WHERE user_id = @user_id AND product_variant_id = @variant_id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@user_id", this.currentUserId);
                            cmd.Parameters.AddWithValue("@variant_id", variantId);
                            object result = cmd.ExecuteScalar();
                            if (result != null)
                            {
                                currentQuantity = Convert.ToInt32(result);
                            }
                        }
                    }
                    UpdateCartItemQuantity(variantId, currentQuantity - 1);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการดึงจำนวนปัจจุบัน: " + ex.Message, "Error");
                }
            }
        }

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int cartItemId)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM cart_items WHERE cart_item_id = @cart_item_id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@cart_item_id", cartItemId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadCartItems();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบสินค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        private void btnCheckout_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null && totalPrice > 0)
            {
                SlideManage.Instance.NavigateWithSlide(new CheckoutConfirmationPage(parent, this.totalPrice), false);
            }
        }

        // --- (Window Control & Top Nav - เหมือนเดิม) ---

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
                SlideManage.Instance.NavigateWithSlide(new CartPage(parent), true);
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
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), true);
        }
        private void btnProfile_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
                SlideManage.Instance.NavigateWithSlide(new UserProfilePage(parent), false);
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