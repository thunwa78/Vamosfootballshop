using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace login_store
{
    public class CheckoutItem : CartItemDetails
    {
        public decimal SubTotal => Price * Quantity;
    }

    public partial class CheckoutConfirmationPage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int currentUserId;
        private decimal cartTotalPrice;

        private ObservableCollection<CheckoutItem> checkoutItems;
        private UserAddress selectedAddress;
        private ObservableCollection<VoucherDisplay> availableVouchers = new ObservableCollection<VoucherDisplay>();

        // (คงค่า VAT Rate ไว้คำนวณย้อนกลับ)
        private const decimal VAT_RATE = 0.07m;

        public CheckoutConfirmationPage(SlideManage parent, decimal totalPriceFromCart)
        {
            InitializeComponent();
            this.parent = parent;
            this.currentUserId = UserSession.UserId;
            this.cartTotalPrice = totalPriceFromCart;
            checkoutItems = new ObservableCollection<CheckoutItem>();
            CartItemsDataGrid.ItemsSource = checkoutItems;
            cmbMyVouchers.ItemsSource = availableVouchers;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCartItems();
            LoadUserAddress();
            LoadMyVouchers();
            CalculateTotals(null);
        }

        // --- 1. โหลดที่อยู่ User (เหมือนเดิม) ---
        private void LoadUserAddress()
        {
            selectedAddress = null;
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM user_addresses WHERE user_id = @userId ORDER BY is_default DESC LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                selectedAddress = new UserAddress
                                {
                                    FullName = GetStringSafe(reader, "full_name"),
                                    PhoneNumber = GetStringSafe(reader, "phone_number"),
                                    AddressLine1 = GetStringSafe(reader, "address_line1"),
                                    SubDistrict = GetStringSafe(reader, "sub_district"),
                                    District = GetStringSafe(reader, "district"),
                                    Province = GetStringSafe(reader, "province"),
                                    PostalCode = GetStringSafe(reader, "postal_code")
                                };
                                txtAddressName.Text = $"{selectedAddress.FullName} ({selectedAddress.PhoneNumber})";
                                txtAddressDetail.Text = $"{selectedAddress.AddressLine1}, {selectedAddress.SubDistrict}, {selectedAddress.District}, {selectedAddress.Province} {selectedAddress.PostalCode}";
                                btnProceed.IsEnabled = true;
                            }
                            else
                            {
                                txtAddressName.Text = "🚨 กรุณาเพิ่มที่อยู่จัดส่ง";
                                txtAddressDetail.Text = "ไปที่หน้า 'โปรไฟล์' > 'ที่อยู่ของฉัน' เพื่อเพิ่มที่อยู่";
                                btnProceed.IsEnabled = false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดที่อยู่จัดส่ง: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 2. โหลดรายการสินค้า (เหมือนเดิม) ---
        private void LoadCartItems()
        {
            checkoutItems.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT ci.cart_item_id, p.product_id, pv.variant_id AS product_variant_id,
                            p.name, pv.size_name, p.price, ci.quantity 
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
                                checkoutItems.Add(new CheckoutItem
                                {
                                    CartItemId = reader.GetInt32("cart_item_id"),
                                    ProductId = reader.GetInt32("product_id"),
                                    VariantId = reader.GetInt32("product_variant_id"),
                                    Name = reader.GetString("name"),
                                    Price = reader.GetDecimal("price"),
                                    Quantity = reader.GetInt32("quantity"),
                                    SizeName = reader.GetString("size_name")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลตะกร้าได้: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            if (!checkoutItems.Any())
            {
                parent.NavigateWithSlide(new CartPage(parent), true);
            }
        }

        // --- 3. โหลดโค้ดส่วนลด (เหมือนเดิม) ---
        private void LoadMyVouchers()
        {
            availableVouchers.Clear();
            availableVouchers.Add(new VoucherDisplay { VoucherId = 0, Code = "ไม่ใช้ส่วนลด", Description = "ชำระเงินเต็มจำนวน", DiscountAmount = 0, DiscountType = "Fixed" });
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT v.*
                        FROM user_vouchers uv
                        JOIN vouchers v ON uv.voucher_id = v.voucher_id
                        WHERE uv.user_id = @userId 
                          AND uv.is_used = 0 
                          AND v.valid_to > NOW()
                          AND v.min_purchase <= @cartTotal";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", this.currentUserId);
                        cmd.Parameters.AddWithValue("@cartTotal", this.cartTotalPrice);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                availableVouchers.Add(new VoucherDisplay
                                {
                                    VoucherId = reader.GetInt32("voucher_id"),
                                    Code = reader.GetString("code"),
                                    Description = reader.GetString("description"),
                                    DiscountAmount = reader.GetDecimal("discount_amount"),
                                    DiscountType = reader.IsDBNull(reader.GetOrdinal("discount_type")) ? "Fixed" : reader.GetString("discount_type"),
                                    MinPurchase = reader.GetDecimal("min_purchase"),
                                    ValidTo = reader.GetDateTime("valid_to")
                                });
                            }
                        }
                    }
                }
                cmbMyVouchers.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดโค้ดส่วนลด: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // --- 4. คำนวณยอด (แก้ไข: เปลี่ยนสูตรคำนวณ) ---
        private void CalculateTotals(VoucherDisplay voucher)
        {
            decimal grossTotal = this.cartTotalPrice; // ราคาสินค้า (16,890)
            decimal discountVal = 0;

            // 1. คำนวณส่วนลด
            if (voucher != null && voucher.VoucherId > 0)
            {
                if (voucher.DiscountType == "Percentage")
                {
                    discountVal = grossTotal * (voucher.DiscountAmount / 100); // 30% = 5,067
                }
                else
                {
                    discountVal = voucher.DiscountAmount;
                }
            }
            discountVal = Math.Min(discountVal, grossTotal);

            // 2. ยอดหลังหักส่วนลด (แต่ยังไม่รวม VAT)
            decimal amountBeforeVat = grossTotal - discountVal; // 16,890 - 5,067 = 11,823

            // 3. คำนวณ VAT 7% (บวกเพิ่ม)
            decimal vatAmount = amountBeforeVat * 0.07m; // 11,823 * 0.07 = 827.61

            // 4. ยอดสุทธิที่ต้องจ่าย (Net Total)
            decimal finalAmount = amountBeforeVat + vatAmount; // 11,823 + 827.61 = 12,650.61

            // แสดงผล
            txtSubTotal.Text = $"{grossTotal:N2} บาท";
            txtDiscountValue.Text = $"-{discountVal:N2} บาท";
            txtVAT.Text = $"{vatAmount:N2} บาท"; // แสดงยอด VAT ที่บวกเพิ่ม
            txtFinalTotal.Text = $"{finalAmount:N2} บาท"; // ยอดที่ลูกค้าต้องโอนจริง
        }

        // --- 5. ComboBox Event ---
        private void cmbMyVouchers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            VoucherDisplay selectedVoucher = cmbMyVouchers.SelectedItem as VoucherDisplay;
            string discountLabel = "ส่วนลดที่ใช้:";

            if (selectedVoucher != null && selectedVoucher.VoucherId > 0)
            {
                if (selectedVoucher.DiscountType == "Percentage")
                {
                    discountLabel = $"ส่วนลด: {selectedVoucher.Code} ({selectedVoucher.DiscountAmount:N0}%)";
                }
                else
                {
                    discountLabel = $"ส่วนลด: {selectedVoucher.Code}";
                }
            }

            txtDiscountLabel.Text = discountLabel;
            CalculateTotals(selectedVoucher);
        }

        // --- 6. ปุ่ม "ดำเนินการต่อ" (แก้ไข: คำนวณให้ตรงกัน) ---
        private void btnProceedToPayment_Click(object sender, RoutedEventArgs e)
        {
            if (selectedAddress == null)
            {
                CustomMessageBoxWindow.Show("กรุณาเพิ่มที่อยู่จัดส่งในหน้าโปรไฟล์ก่อนดำเนินการต่อ", "Validation", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            VoucherDisplay selectedVoucher = cmbMyVouchers.SelectedItem as VoucherDisplay;
            int voucherIdToUse = 0;
            decimal discountAmountInBaht = 0;

            if (selectedVoucher != null && selectedVoucher.VoucherId > 0)
            {
                voucherIdToUse = selectedVoucher.VoucherId;
                if (selectedVoucher.DiscountType == "Percentage")
                {
                    discountAmountInBaht = this.cartTotalPrice * (selectedVoucher.DiscountAmount / 100);
                }
                else
                {
                    discountAmountInBaht = selectedVoucher.DiscountAmount;
                }
                discountAmountInBaht = Math.Min(discountAmountInBaht, this.cartTotalPrice);
            }

            // (สำคัญ) อ่านยอดสุทธิ (ที่รวม VAT แล้ว) จากหน้าจอ เพื่อส่งไปบันทึกเป็น TotalAmount ใน Database
            decimal finalAmount = decimal.Parse(txtFinalTotal.Text.Replace(" บาท", "").Replace(",", ""));

            if (parent != null)
            {
                // ส่งยอด finalAmount (12,650.61) ไปให้หน้า CheckoutPage
                parent.NavigateWithSlide(new CheckoutPage(parent, this.cartTotalPrice, finalAmount, discountAmountInBaht, voucherIdToUse), false);
            }
        }

        // --- (Helper & Standard Methods - เหมือนเดิม) ---
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
                LoadMyVouchers();
                CalculateTotals(null);
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
                        if (result != null) maxStock = Convert.ToInt32(result);
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
                LoadMyVouchers();
                CalculateTotals(null);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการอัปเดตจำนวนสินค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void btnManageAddress_Click(object sender, RoutedEventArgs e) { if (parent != null) parent.NavigateWithSlide(new UserProfilePage(parent), false); }
        private void BtnIncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int variantId)
            {
                var item = checkoutItems.FirstOrDefault(i => i.VariantId == variantId);
                if (item != null) UpdateCartItemQuantity(variantId, item.Quantity + 1);
            }
        }
        private void BtnDecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int variantId)
            {
                var item = checkoutItems.FirstOrDefault(i => i.VariantId == variantId);
                if (item != null) UpdateCartItemQuantity(variantId, item.Quantity - 1);
            }
        }
        private string GetStringSafe(MySqlDataReader reader, string columnName) { int colIndex = reader.GetOrdinal(columnName); if (reader.IsDBNull(colIndex)) return string.Empty; return reader.GetString(colIndex); }

        // (Window Controls & Top Nav - เหมือนเดิม)
        private void MinimizeButton_Click(object sender, RoutedEventArgs e) { Window window = Window.GetWindow(this); if (window != null) window.WindowState = WindowState.Minimized; }
        private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e) { Window window = Window.GetWindow(this); if (window != null) window.WindowState = (window.WindowState == WindowState.Maximized) ? WindowState.Normal : WindowState.Maximized; }
        private void CloseButton_Click(object sender, RoutedEventArgs e) { Application.Current.Shutdown(); }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.Source == sender) { Window.GetWindow(this)?.DragMove(); } }
        private void Logo_Click(object sender, MouseButtonEventArgs e) { btnShop_Click(sender, new RoutedEventArgs()); }
        private void btnShop_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new ShopPage(parent), true); }
        private void btnCart_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new CartPage(parent), true); }
        private void btnOrders_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new OrdersPage(parent), true); }
        private void btnNotifications_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new NotificationsPage(parent), true); }
        private void btnWishlist_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), false); }
        private void btnProfile_Click(object sender, RoutedEventArgs e) { if (SlideManage.Instance != null) SlideManage.Instance.NavigateWithSlide(new UserProfilePage(parent), false); }

        private void btnVouchers_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new MyVouchersPage(parent), false);
            }
        }

        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new AboutUsPage(parent), false);
            }
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            UserSession.EndSession();
            if (parent != null)
            {
                parent.NavigateWithSlide(new LoginPage(parent), true);
            }
        }
    }
}