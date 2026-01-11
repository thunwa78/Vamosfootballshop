using MySqlConnector;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace login_store
{
    public partial class AddEditVoucherWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private int editVoucherId = 0;

        public AddEditVoucherWindow(int voucherId = 0)
        {
            InitializeComponent();
            this.editVoucherId = voucherId;
            txtTitle.Text = (voucherId == 0) ? "สร้างโค้ดส่วนลดใหม่" : "แก้ไขโค้ดส่วนลด";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dpValidFrom.SelectedDate = DateTime.Today;
            dpValidTo.SelectedDate = DateTime.Today.AddDays(30);

            if (editVoucherId > 0)
            {
                LoadVoucherData();
            }
        }

        // (โหลดข้อมูลเก่ามาแสดง)
        private void LoadVoucherData()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM vouchers WHERE voucher_id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.editVoucherId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtCode.Text = reader.GetString("code");
                                txtDescription.Text = reader.GetString("description");

                                // (แสดงตัวเลขส่วนลด)
                                txtDiscountAmount.Text = reader.GetDecimal("discount_amount").ToString("0.##");

                                txtMinPurchase.Text = reader.GetDecimal("min_purchase").ToString("0.##");
                                dpValidFrom.SelectedDate = reader.GetDateTime("valid_from");
                                dpValidTo.SelectedDate = reader.GetDateTime("valid_to");
                                chkIsActive.IsChecked = reader.GetBoolean("is_active");

                                // (สำคัญ) 👈 โหลดค่า "ประเภท" กลับมาเลือกใน Dropdown
                                string type = reader.IsDBNull(reader.GetOrdinal("discount_type")) ? "Fixed" : reader.GetString("discount_type");

                                if (type == "Percentage")
                                {
                                    cmbDiscountType.SelectedIndex = 1; // เลือก "เปอร์เซ็นต์ (%)"
                                }
                                else
                                {
                                    cmbDiscountType.SelectedIndex = 0; // เลือก "จำนวนเงิน (บาท)"
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลโค้ด: " + ex.Message, "Error");
                this.Close();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // 1. ตรวจสอบความครบถ้วน
            if (string.IsNullOrWhiteSpace(txtCode.Text) ||
                string.IsNullOrWhiteSpace(txtDiscountAmount.Text) ||
                dpValidFrom.SelectedDate == null ||
                dpValidTo.SelectedDate == null)
            {
                CustomMessageBoxWindow.Show("กรุณากรอกข้อมูลให้ครบ", "ข้อมูลไม่ครบ", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            // 2. ตรวจสอบตัวเลข
            if (!decimal.TryParse(txtDiscountAmount.Text, out decimal discount) ||
                !decimal.TryParse(txtMinPurchase.Text, out decimal minPurchase))
            {
                CustomMessageBoxWindow.Show("ตัวเลขไม่ถูกต้อง", "ข้อมูลผิดพลาด", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            // 3. (สำคัญ) ตรวจสอบประเภทและค่า %
            // ถ้าเลือก index 1 แปลว่าเป็น Percentage
            string discountType = (cmbDiscountType.SelectedIndex == 1) ? "Percentage" : "Fixed";

            if (discountType == "Percentage")
            {
                if (discount > 100)
                {
                    CustomMessageBoxWindow.Show("ส่วนลดเปอร์เซ็นต์ต้องไม่เกิน 100%", "ข้อมูลผิดพลาด", CustomMessageBoxWindow.MessageBoxType.Warning);
                    return;
                }
                if (discount <= 0)
                {
                    CustomMessageBoxWindow.Show("ส่วนลดต้องมากกว่า 0%", "ข้อมูลผิดพลาด", CustomMessageBoxWindow.MessageBoxType.Warning);
                    return;
                }
            }

            DateTime validFrom = dpValidFrom.SelectedDate.Value;
            DateTime validTo = dpValidTo.SelectedDate.Value.Date.AddDays(1).AddSeconds(-1);

            if (validTo < validFrom)
            {
                CustomMessageBoxWindow.Show("วันหมดอายุต้องอยู่หลังวันที่เริ่มใช้", "ข้อมูลผิดพลาด", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql;
                    if (editVoucherId == 0)
                    {
                        // (เพิ่ม) บันทึก discount_type ลงใน INSERT
                        sql = @"INSERT INTO vouchers (code, description, discount_amount, discount_type, min_purchase, valid_from, valid_to, is_active)
                                VALUES (@code, @desc, @discount, @type, @min, @from, @to, @active)";
                    }
                    else
                    {
                        // (เพิ่ม) บันทึก discount_type ลงใน UPDATE
                        sql = @"UPDATE vouchers SET code = @code, description = @desc, discount_amount = @discount, discount_type = @type,
                                min_purchase = @min, valid_from = @from, valid_to = @to, is_active = @active
                                WHERE voucher_id = @id";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", this.editVoucherId);
                        cmd.Parameters.AddWithValue("@code", txtCode.Text.Trim().ToUpper());
                        cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@discount", discount);
                        cmd.Parameters.AddWithValue("@type", discountType); // 👈 ส่งค่า "Fixed" หรือ "Percentage"
                        cmd.Parameters.AddWithValue("@min", minPurchase);
                        cmd.Parameters.AddWithValue("@from", validFrom);
                        cmd.Parameters.AddWithValue("@to", validTo);
                        cmd.Parameters.AddWithValue("@active", chkIsActive.IsChecked);
                        cmd.ExecuteNonQuery();
                    }
                }
                CustomMessageBoxWindow.Show("บันทึกสำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                this.DialogResult = true;
                this.Close();
            }
            catch (MySqlException ex) when (ex.Number == 1062)
            {
                CustomMessageBoxWindow.Show("โค้ดนี้มีอยู่แล้ว", "Duplicate Code", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("Error: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e) { this.DialogResult = false; this.Close(); }
        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) { if (e.ButtonState == MouseButtonState.Pressed) this.DragMove(); }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e) { e.Handled = new Regex("[^0-9.]+").IsMatch(e.Text); }
    }
}