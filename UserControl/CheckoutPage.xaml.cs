//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using QRCoder; // 👈 (ต้องมี)
using System.IO;
using System.Drawing; // 👈 (ต้องมี)
using Microsoft.Win32;
using System.Linq;

namespace login_store
{
    public partial class CheckoutPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;

        private decimal cartTotalPrice;
        private decimal finalAmount;
        private decimal discountAmount;
        private int usedVoucherId;

        private string selectedSlipPath = null;

        // (Constructor 5 arguments ที่ถูกต้อง)
        public CheckoutPage(SlideManage parent, decimal cartTotalPrice, decimal finalAmount, decimal discountAmount, int voucherIdToUse)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;

            this.cartTotalPrice = cartTotalPrice;
            this.finalAmount = finalAmount;
            this.discountAmount = discountAmount;
            this.usedVoucherId = voucherIdToUse;

            txtAmountToPay.Text = $"{this.finalAmount:N2} บาท";
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            GenerateQrCode();
        }

        // --- เมธอดสร้าง QR Code ---
        private void GenerateQrCode()
        {
            string promptPayAccount = "099XXXXXXX"; // (แก้เป็นเบอร์ของคุณ)
            string payload = $"00020101021129370016A0000006770101110113{promptPayAccount.Length:D2}{promptPayAccount}5303764540{this.finalAmount.ToString("F2").Length:D2}{this.finalAmount:F2}5802TH6304";

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap qrCodeBitmap = qrCode.GetGraphic(20))
            {
                qrCodeImage.Source = ConvertBitmapToBitmapImage(qrCodeBitmap);
            }
        }

        private BitmapImage ConvertBitmapToBitmapImage(Bitmap bitmap)
        {
            try
            {
                using (MemoryStream memory = new MemoryStream())
                {
                    bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Bmp);
                    memory.Position = 0;
                    BitmapImage bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    return bitmapImage;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("QR Code Conversion Error: " + ex.Message);
                return new BitmapImage();
            }
        }

        // --- Event Handler สำหรับ Browse Slip ---
        private void btnBrowseSlip_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "แนบหลักฐานการโอนเงิน";
            op.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg";

            if (op.ShowDialog() == true)
            {
                selectedSlipPath = op.FileName;

                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(selectedSlipPath, UriKind.Absolute);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    imgSlipPreview.Source = bitmap;
                    txtSlipPlaceholder.Visibility = Visibility.Collapsed;
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("ไม่สามารถโหลดรูปพรีวิว: " + ex.Message, "Error");
                }
            }
        }


        // --- ปุ่มยืนยัน (btnConfirmOrder_Click) ---
        private void btnConfirmOrder_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedSlipPath) || !File.Exists(selectedSlipPath))
            {
                CustomMessageBoxWindow.Show("กรุณาแนบหลักฐานการโอนเงิน (สลิป)", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            MySqlConnection conn = null;
            MySqlTransaction transaction = null;
            string dbSlipPath = null;

            try
            {
                // --- 1. จัดการ File Upload (คัดลอกไฟล์สลิป) ---
                string uniqueFileName = $"slip_{currentUserId}_{Guid.NewGuid().ToString()}{Path.GetExtension(selectedSlipPath)}";
                string slipImagesDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Images", "Slips");
                if (!Directory.Exists(slipImagesDir)) Directory.CreateDirectory(slipImagesDir);

                string destinationFilePath = System.IO.Path.Combine(slipImagesDir, uniqueFileName);
                System.IO.File.Copy(selectedSlipPath, destinationFilePath, true);

                dbSlipPath = $"/Images/Slips/{uniqueFileName}";

                // --- 2. สร้าง Transaction ---
                conn = new MySqlConnection(connectionString);
                conn.Open();
                transaction = conn.BeginTransaction();

                // 2.1 เพิ่ม 'discount_amount' และ 'used_voucher_id'
                string sqlInsertOrder = "INSERT INTO orders (user_id, order_date, total_amount, status, slip_path, discount_amount, used_voucher_id) " +
                                        "VALUES (@user_id, NOW(), @total_amount, 'Pending Approval', @slip_path, @discount, @voucherId); SELECT LAST_INSERT_ID();";

                long newOrderId;
                using (MySqlCommand cmdOrder = new MySqlCommand(sqlInsertOrder, conn, transaction))
                {
                    cmdOrder.Parameters.AddWithValue("@user_id", this.currentUserId);
                    cmdOrder.Parameters.AddWithValue("@total_amount", this.finalAmount);
                    cmdOrder.Parameters.AddWithValue("@slip_path", dbSlipPath);
                    cmdOrder.Parameters.AddWithValue("@discount", this.discountAmount);
                    cmdOrder.Parameters.AddWithValue("@voucherId", (this.usedVoucherId > 0) ? (object)this.usedVoucherId : DBNull.Value);
                    newOrderId = Convert.ToInt64(cmdOrder.ExecuteScalar());
                }

                // 2.2 ดึงสินค้าในตะกร้า & ลดสต็อก
                string sqlSelectCart = @"
                    SELECT 
                        pv.product_id, 
                        ci.product_variant_id, 
                        ci.quantity, 
                        p.price 
                    FROM cart_items ci
                    JOIN product_variants pv ON ci.product_variant_id = pv.variant_id
                    JOIN products p ON pv.product_id = p.product_id
                    WHERE ci.user_id = @user_id";

                var itemsToMove = new List<Tuple<int, int, int, decimal>>();

                using (MySqlCommand cmdSelectCart = new MySqlCommand(sqlSelectCart, conn, transaction))
                {
                    cmdSelectCart.Parameters.AddWithValue("@user_id", this.currentUserId);
                    using (var reader = cmdSelectCart.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            itemsToMove.Add(Tuple.Create(
                                reader.GetInt32("product_id"),
                                reader.GetInt32("product_variant_id"),
                                reader.GetInt32("quantity"),
                                reader.GetDecimal("price")
                            ));
                        }
                    }
                }

                // 2.3 ลดสต็อก Variant และ ย้ายเข้า order_items
                string sqlUpdateVariantStock = "UPDATE product_variants SET stock_quantity = stock_quantity - @qty WHERE variant_id = @variant_id AND stock_quantity >= @qty";
                string sqlInsertItem = "INSERT INTO order_items (order_id, product_variant_id, quantity, price_at_purchase) VALUES (@order_id, @variant_id, @quantity, @price)";
                var productsToUpdateStock = new HashSet<int>();

                foreach (var item in itemsToMove)
                {
                    int productId = item.Item1;
                    int variantId = item.Item2;
                    int quantity = item.Item3;
                    decimal price = item.Item4;

                    productsToUpdateStock.Add(productId);

                    // ลดสต็อก Variant
                    using (MySqlCommand cmdUpdate = new MySqlCommand(sqlUpdateVariantStock, conn, transaction))
                    {
                        cmdUpdate.Parameters.AddWithValue("@qty", quantity);
                        cmdUpdate.Parameters.AddWithValue("@variant_id", variantId);
                        int rowsAffected = cmdUpdate.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            transaction.Rollback();
                            CustomMessageBoxWindow.Show("การทำรายการล้มเหลว: สินค้าบางรายการในตะกร้าถูกซื้อไปแล้วหรือสต็อกไม่พอ", "สต็อกไม่พอ", CustomMessageBoxWindow.MessageBoxType.Error);
                            return;
                        }
                    }

                    // ย้ายเข้า order_items
                    using (MySqlCommand cmdInsertItem = new MySqlCommand(sqlInsertItem, conn, transaction))
                    {
                        cmdInsertItem.Parameters.AddWithValue("@order_id", newOrderId);
                        cmdInsertItem.Parameters.AddWithValue("@variant_id", variantId);
                        cmdInsertItem.Parameters.AddWithValue("@quantity", quantity);
                        cmdInsertItem.Parameters.AddWithValue("@price", price);
                        cmdInsertItem.ExecuteNonQuery();
                    }
                }

                // 2.4 อัปเดตสต็อกรวมในตาราง products
                string sqlRecalculateStock = "UPDATE products p SET p.stock_quantity = (SELECT SUM(pv.stock_quantity) FROM product_variants pv WHERE pv.product_id = p.product_id) WHERE p.product_id = @product_id";
                foreach (int productId in productsToUpdateStock)
                {
                    using (MySqlCommand cmdRecalculate = new MySqlCommand(sqlRecalculateStock, conn, transaction))
                    {
                        cmdRecalculate.Parameters.AddWithValue("@product_id", productId);
                        cmdRecalculate.ExecuteNonQuery();
                    }
                }

                // 2.5 ตัดสิทธิ์ Voucher ที่ใช้แล้ว
                if (this.usedVoucherId > 0)
                {
                    string sqlUpdateVoucher = "UPDATE user_vouchers SET is_used = 1 WHERE user_id = @user_id AND voucher_id = @voucherId AND is_used = 0";
                    using (MySqlCommand cmdVoucher = new MySqlCommand(sqlUpdateVoucher, conn, transaction))
                    {
                        cmdVoucher.Parameters.AddWithValue("@user_Id", this.currentUserId);
                        cmdVoucher.Parameters.AddWithValue("@voucherId", this.usedVoucherId);
                        cmdVoucher.ExecuteNonQuery();
                    }
                }

                // 2.6 ล้างตะกร้า (cart_items)
                string sqlDeleteCart = "DELETE FROM cart_items WHERE user_id = @user_id";
                using (MySqlCommand cmdDeleteCart = new MySqlCommand(sqlDeleteCart, conn, transaction))
                {
                    cmdDeleteCart.Parameters.AddWithValue("@user_id", this.currentUserId);
                    cmdDeleteCart.ExecuteNonQuery();
                }

                // 3. Commit
                transaction.Commit();
                CustomMessageBoxWindow.Show("ได้รับหลักฐานการโอนเงินแล้ว! คำสั่งซื้อของคุณอยู่ระหว่างการตรวจสอบ", "สำเร็จ", CustomMessageBoxWindow.MessageBoxType.Success);

                if (SlideManage.Instance != null)
                    SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), false);
            }
            catch (Exception ex)
            {
                transaction?.Rollback();
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการสร้างคำสั่งซื้อ: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            finally
            {
                conn?.Close();
            }
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
            if (e.OriginalSource.GetType() == typeof(Border) || e.OriginalSource.GetType() == typeof(Grid) || e.OriginalSource.GetType() == typeof(ScrollViewer))
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    Window.GetWindow(this)?.DragMove();
                }
            }
        }

        // (แก้) 👈 (สำคัญ) แก้ชื่อเมธอดนี้ให้ตรงกับ XAML
        private void btnBackToConfirmation_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                // (เราต้องส่ง 'cartTotalPrice' (ยอดก่อนหักส่วนลด) กลับไป)
                SlideManage.Instance.NavigateWithSlide(new CheckoutConfirmationPage(parent, this.cartTotalPrice), true);
            }
        }
    }
}