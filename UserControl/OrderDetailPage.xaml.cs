using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;
using MySqlConnector;
using System.Collections.Generic;
using Microsoft.Win32;
using System.Diagnostics;
using System.Text;
using System.Windows.Media.Imaging;
using System.Linq;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace login_store
{
    public partial class OrderDetailPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;
        private int orderId;

        private OrderDetails currentOrder;
        private List<CartItemDetails> currentOrderItems = new List<CartItemDetails>();

        public OrderDetailPage(SlideManage parent, int orderId)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
            this.orderId = orderId;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadOrderDetails();
        }

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
            catch (Exception) { }
            return null;
        }

        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex)) return string.Empty;
            return reader.GetString(colIndex);
        }

        private CompanyInfo GetCompanyInfo()
        {
            CompanyInfo info = new CompanyInfo();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM company_info WHERE id = 1 LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            info.Description = GetStringSafe(reader, "description");
                            info.Email = GetStringSafe(reader, "email");
                            info.Phone = GetStringSafe(reader, "phone");
                            info.TaxId = GetStringSafe(reader, "tax_id");
                            info.Address = GetStringSafe(reader, "address");
                        }
                    }
                }
            }
            catch { }
            return info;
        }

        // --- เมธอดหลัก: โหลดรายละเอียดบิล ---
        private void LoadOrderDetails()
        {
            currentOrderItems.Clear();
            if (ItemsPanel == null) return;
            ItemsPanel.Children.Clear();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // 1. ดึงข้อมูล
                    string sqlOrder = @"
                        SELECT o.order_date, o.total_amount, o.status, o.discount_amount, v.code 
                        FROM orders o
                        LEFT JOIN vouchers v ON o.used_voucher_id = v.voucher_id
                        WHERE o.order_id = @order_id AND o.user_id = @user_id";

                    using (MySqlCommand cmdOrder = new MySqlCommand(sqlOrder, conn))
                    {
                        cmdOrder.Parameters.AddWithValue("@order_id", this.orderId);
                        cmdOrder.Parameters.AddWithValue("@user_id", this.currentUserId);
                        using (var reader = cmdOrder.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                currentOrder = new OrderDetails
                                {
                                    OrderId = this.orderId,
                                    OrderDate = reader.GetDateTime("order_date"),
                                    TotalAmount = reader.GetDecimal("total_amount"),
                                    Status = reader.GetString("status"),
                                    DiscountAmount = reader.IsDBNull(reader.GetOrdinal("discount_amount")) ? 0 : reader.GetDecimal("discount_amount"),
                                    VoucherCode = reader.IsDBNull(reader.GetOrdinal("code")) ? "" : reader.GetString("code")
                                };

                                txtOrderId.Text = $"Order #{currentOrder.OrderId}";
                                txtOrderDate.Text = $"วันที่: {currentOrder.OrderDate:dd MMM yyyy HH:mm}";
                                txtOrderStatus.Text = currentOrder.Status;
                                txtTotalPrice.Text = $"ยอดรวมทั้งหมด: {currentOrder.TotalAmount:N2} บาท";

                                // (แก้) เหลือแค่การเปลี่ยนสี ไม่มีการเรียก btnCancelOrder
                                if (currentOrder.Status == "Pending Payment" || currentOrder.Status == "Pending Approval")
                                    txtOrderStatus.Foreground = Brushes.Orange;
                                else if (currentOrder.Status == "Approved" || currentOrder.Status == "Shipped")
                                    txtOrderStatus.Foreground = Brushes.LightGreen;
                                else if (currentOrder.Status == "Cancelled" || currentOrder.Status == "Rejected")
                                    txtOrderStatus.Foreground = Brushes.Red;
                                else
                                    txtOrderStatus.Foreground = Brushes.White;
                            }
                            else
                            {
                                CustomMessageBoxWindow.Show("ไม่พบคำสั่งซื้อนี้", "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                                BackToOrders_Click(null, null);
                                return;
                            }
                        }
                    }

                    // 2. ดึงรายการสินค้า (SQL ที่ถูกต้อง)
                    string sqlItems = @"
                        SELECT 
                            p.name, 
                            p.image_path, 
                            pv.size_name, 
                            oi.quantity, 
                            oi.price_at_purchase
                        FROM order_items oi 
                        LEFT JOIN product_variants pv ON oi.product_variant_id = pv.variant_id
                        LEFT JOIN products p ON p.product_id = COALESCE(pv.product_id, oi.product_id)
                        WHERE oi.order_id = @order_id";

                    using (MySqlCommand cmdItems = new MySqlCommand(sqlItems, conn))
                    {
                        cmdItems.Parameters.AddWithValue("@order_id", this.orderId);
                        using (var reader = cmdItems.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                CartItemDetails item = new CartItemDetails
                                {
                                    Name = GetStringSafe(reader, "name"),
                                    SizeName = GetStringSafe(reader, "size_name"),
                                    Quantity = reader.GetInt32("quantity"),
                                    Price = reader.GetDecimal("price_at_purchase"),
                                    ImagePath = GetStringSafe(reader, "image_path")
                                };
                                item.ImagePreview = LoadImagePreview(item.ImagePath);
                                currentOrderItems.Add(item);
                                ItemsPanel.Children.Add(CreateItemRow(item));
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดรายละเอียดคำสั่งซื้อได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private Grid CreateItemRow(CartItemDetails item)
        {
            Grid grid = new Grid { Margin = new Thickness(0, 5, 0, 10) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            System.Windows.Controls.Image productImage = new System.Windows.Controls.Image
            {
                Width = 50,
                Height = 50,
                Stretch = Stretch.Uniform,
                Source = item.ImagePreview
            };
            Grid.SetColumn(productImage, 0);
            grid.Children.Add(productImage);

            StackPanel namePanel = new StackPanel { VerticalAlignment = System.Windows.VerticalAlignment.Center, Margin = new Thickness(10, 0, 0, 0) };
            namePanel.Children.Add(new TextBlock { Text = item.Name, Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.SemiBold });
            if (!string.IsNullOrEmpty(item.SizeName))
            {
                namePanel.Children.Add(new TextBlock { Text = $"ขนาด: {item.SizeName}", Foreground = Brushes.LightGray, FontSize = 12 });
            }
            Grid.SetColumn(namePanel, 1);
            grid.Children.Add(namePanel);

            TextBlock qty = new TextBlock { Text = $"x {item.Quantity}", Foreground = Brushes.LightGray, FontSize = 14, Margin = new Thickness(10, 0, 10, 0), VerticalAlignment = System.Windows.VerticalAlignment.Center };
            Grid.SetColumn(qty, 2);
            grid.Children.Add(qty);

            TextBlock price = new TextBlock { Text = $"{(item.Price * item.Quantity):N2} บ.", Foreground = Brushes.White, FontSize = 14, FontWeight = FontWeights.SemiBold, VerticalAlignment = System.Windows.VerticalAlignment.Center };
            Grid.SetColumn(price, 3);
            grid.Children.Add(price);

            return grid;
        }

        private void BtnDownloadBill_Click(object sender, RoutedEventArgs e)
        {
            if (currentOrder == null || currentOrderItems.Count == 0)
            {
                CustomMessageBoxWindow.Show("ไม่พบข้อมูลคำสั่งซื้อ", "Error", CustomMessageBoxWindow.MessageBoxType.Error);
                return;
            }

            QuestPDF.Settings.License = LicenseType.Community;
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "PDF Document (*.pdf)|*.pdf";
            saveDialog.FileName = $"VAMOS_Invoice_{currentOrder.OrderId}.pdf";

            if (saveDialog.ShowDialog() == true)
            {
                try
                {
                    CompanyInfo shopInfo = GetCompanyInfo();
                    var document = new InvoiceDocument(currentOrder, currentOrderItems, shopInfo);
                    document.GeneratePdf(saveDialog.FileName);
                    Process.Start(new ProcessStartInfo(saveDialog.FileName) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการสร้าง PDF: " + ex.Message, "Save Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // --- Window Control Methods ---
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) { Window window = Window.GetWindow(this); if (window != null) window.WindowState = WindowState.Minimized; }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e) { Window window = Window.GetWindow(this); if (window != null) window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized; }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.Source.GetType() == typeof(Border) || e.Source.GetType() == typeof(Grid) || e.Source.GetType() == typeof(ScrollViewer))
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void BackToOrders_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), true);
            }
        }

        // (Overload เผื่อ XAML เก่าใช้ MouseLeftButtonUp)
        private void BackToOrders_Click(object sender, MouseButtonEventArgs e)
        {
            BackToOrders_Click(sender, new RoutedEventArgs());
        }
    }
}