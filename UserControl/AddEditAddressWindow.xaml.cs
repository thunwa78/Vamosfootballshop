//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace login_store
{
    public partial class AddEditAddressWindow : Window
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        // (เพิ่ม) 1. ตัวแปรสำหรับเก็บที่อยู่ (ถ้าเป็นโหมดแก้ไข)
        private UserAddress addressToEdit = null;
        private int currentUserId;

        // Constructor 1: โหมด "เพิ่มใหม่"
        public AddEditAddressWindow()
        {
            InitializeComponent();
            this.addressToEdit = null; // ไม่มีที่อยู่เดิม
            this.currentUserId = UserSession.UserId;
            txtTitle.Text = "เพิ่มที่อยู่ใหม่";
        }

        // Constructor 2: โหมด "แก้ไข"
        public AddEditAddressWindow(UserAddress addressToEdit)
        {
            InitializeComponent();
            this.addressToEdit = addressToEdit; // รับที่อยู่เดิมมา
            this.currentUserId = UserSession.UserId;
            txtTitle.Text = "แก้ไขที่อยู่";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // ถ้าเป็นโหมด "แก้ไข" ให้โหลดข้อมูลเก่ามาใส่ฟอร์ม
            if (addressToEdit != null)
            {
                LoadAddressData();
            }
        }

        // (เพิ่ม) 2. เมธอดสำหรับโหลดข้อมูลเดิม (โหมดแก้ไข)
        private void LoadAddressData()
        {
            txtAddressLabel.Text = addressToEdit.AddressLabel;
            txtFullName.Text = addressToEdit.FullName;
            txtPhoneNumber.Text = addressToEdit.PhoneNumber;
            txtAddressLine1.Text = addressToEdit.AddressLine1;
            txtSubDistrict.Text = addressToEdit.SubDistrict;
            txtDistrict.Text = addressToEdit.District;
            txtProvince.Text = addressToEdit.Province;
            txtPostalCode.Text = addressToEdit.PostalCode;
            chkIsDefault.IsChecked = addressToEdit.IsDefault;
        }

        // --- 3. ปุ่มบันทึก (สำคัญ) ---
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // (เพิ่ม) 3a. ตรวจสอบข้อมูลเบื้องต้น
            if (string.IsNullOrWhiteSpace(txtAddressLabel.Text) ||
                string.IsNullOrWhiteSpace(txtFullName.Text) ||
                string.IsNullOrWhiteSpace(txtPhoneNumber.Text) ||
                string.IsNullOrWhiteSpace(txtAddressLine1.Text) ||
                string.IsNullOrWhiteSpace(txtSubDistrict.Text) ||
                string.IsNullOrWhiteSpace(txtDistrict.Text) ||
                string.IsNullOrWhiteSpace(txtProvince.Text) ||
                string.IsNullOrWhiteSpace(txtPostalCode.Text))
            {
                CustomMessageBoxWindow.Show("กรุณากรอกข้อมูลที่อยู่ให้ครบทุกช่อง", "ข้อมูลไม่ครบ", CustomMessageBoxWindow.MessageBoxType.Warning);
                return;
            }

            MySqlConnection conn = new MySqlConnection(connectionString);
            MySqlTransaction transaction = null;

            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();

                // (เพิ่ม) 3b. (สำคัญ) ถ้าติ๊ก "ที่อยู่หลัก"
                // เราต้องล้าง "ที่อยู่หลัก" อันอื่นของ User คนนี้ ออกก่อน
                if (chkIsDefault.IsChecked == true)
                {
                    string sqlClearDefault = "UPDATE user_addresses SET is_default = 0 WHERE user_id = @userId AND is_default = 1";
                    using (MySqlCommand cmdClear = new MySqlCommand(sqlClearDefault, conn, transaction))
                    {
                        cmdClear.Parameters.AddWithValue("@userId", this.currentUserId);
                        cmdClear.ExecuteNonQuery();
                    }
                }

                // (เพิ่ม) 3c. บันทึกข้อมูล (INSERT หรือ UPDATE)
                string sqlSave;
                if (addressToEdit == null) // โหมด "เพิ่มใหม่"
                {
                    sqlSave = @"
                        INSERT INTO user_addresses 
                        (user_id, address_label, full_name, phone_number, address_line1, sub_district, district, province, postal_code, is_default)
                        VALUES 
                        (@userId, @label, @name, @phone, @addr1, @subDist, @dist, @prov, @postal, @isDefault)";
                }
                else // โหมด "แก้ไข"
                {
                    sqlSave = @"
                        UPDATE user_addresses SET 
                        address_label = @label, full_name = @name, phone_number = @phone, address_line1 = @addr1, 
                        sub_district = @subDist, district = @dist, province = @prov, postal_code = @postal, is_default = @isDefault
                        WHERE address_id = @addressId AND user_id = @userId";
                }

                using (MySqlCommand cmdSave = new MySqlCommand(sqlSave, conn, transaction))
                {
                    cmdSave.Parameters.AddWithValue("@userId", this.currentUserId);
                    cmdSave.Parameters.AddWithValue("@addressId", addressToEdit?.AddressId); // (จะเป็น null ถ้าเป็นโหมดเพิ่ม)
                    cmdSave.Parameters.AddWithValue("@label", txtAddressLabel.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@name", txtFullName.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@phone", txtPhoneNumber.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@addr1", txtAddressLine1.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@subDist", txtSubDistrict.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@dist", txtDistrict.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@prov", txtProvince.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@postal", txtPostalCode.Text.Trim());
                    cmdSave.Parameters.AddWithValue("@isDefault", chkIsDefault.IsChecked);

                    cmdSave.ExecuteNonQuery();
                }

                // (เพิ่ม) 3d. ยืนยัน Transaction
                transaction.Commit();

                CustomMessageBoxWindow.Show("บันทึกที่อยู่สำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                this.DialogResult = true; // 👈 ส่งสัญญาณว่า "บันทึกสำเร็จ"
                this.Close();
            }
            catch (Exception ex)
            {
                transaction?.Rollback(); // 👈 ยกเลิกการเปลี่ยนแปลงทั้งหมดถ้า Error
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึกที่อยู่: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
            finally
            {
                conn?.Close();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // ตรวจสอบว่าคลิกที่ Border หลัก (ไม่ใช่ปุ่มหรือ TextBox)
            if (e.OriginalSource.GetType() == typeof(Border) || e.OriginalSource.GetType() == typeof(Grid))
            {
                if (e.ButtonState == MouseButtonState.Pressed)
                {
                    this.DragMove();
                }
            }
        }

        // --- 4. ปุ่มปิด/ยกเลิก ---
        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // 👈 ส่งสัญญาณว่า "ยกเลิก"
            this.Close();
        }
    }
}