//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Collections.Generic;
using System.Collections.ObjectModel; // 👈 (ต้องมี)

namespace login_store
{
    public partial class UserProfilePage : Page
    {
        private SlideManage parent;
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private string defaultImagePath = "/Images/default_profile.png";

        private ObservableCollection<UserAddress> userAddresses = new ObservableCollection<UserAddress>();

        public UserProfilePage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            AddressesListBox.ItemsSource = userAddresses;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUserProfile();
            LoadAddresses();
        }

        private void LoadUserProfile()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = @"
                        SELECT username, email, first_name, last_name, phone_number,
                               profile_image_path
                        FROM users 
                        WHERE id = @userId";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", UserSession.UserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                txtUsername.Text = GetStringSafe(reader, "username");
                                txtEmail.Text = GetStringSafe(reader, "email");
                                txtFirstName.Text = GetStringSafe(reader, "first_name");
                                txtLastName.Text = GetStringSafe(reader, "last_name");
                                txtPhoneNumber.Text = GetStringSafe(reader, "phone_number");
                                string profilePath = GetStringSafe(reader, "profile_image_path");
                                LoadProfileImage(profilePath);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลโปรไฟล์: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        private void LoadAddresses()
        {
            userAddresses.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM user_addresses WHERE user_id = @userId ORDER BY is_default DESC, address_label ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", UserSession.UserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                userAddresses.Add(new UserAddress
                                {
                                    AddressId = reader.GetInt32("address_id"),
                                    UserId = reader.GetInt32("user_id"),
                                    AddressLabel = GetStringSafe(reader, "address_label"),
                                    FullName = GetStringSafe(reader, "full_name"),
                                    PhoneNumber = GetStringSafe(reader, "phone_number"),
                                    AddressLine1 = GetStringSafe(reader, "address_line1"),
                                    SubDistrict = GetStringSafe(reader, "sub_district"),
                                    District = GetStringSafe(reader, "district"),
                                    Province = GetStringSafe(reader, "province"),
                                    PostalCode = GetStringSafe(reader, "postal_code"),
                                    IsDefault = reader.GetBoolean("is_default")
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดที่อยู่: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }


        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE users SET first_name = @firstName, last_name = @lastName, phone_number = @phone WHERE id = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@firstName", txtFirstName.Text.Trim());
                        cmd.Parameters.AddWithValue("@lastName", txtLastName.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", txtPhoneNumber.Text.Trim());
                        cmd.Parameters.AddWithValue("@userId", UserSession.UserId);

                        cmd.ExecuteNonQuery();
                        CustomMessageBoxWindow.Show("บันทึกข้อมูลส่วนตัวสำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถบันทึกข้อมูล: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (แก้) 👈 1. แก้ไขปุ่ม "เพิ่ม"
        private void btnAddAddress_Click(object sender, RoutedEventArgs e)
        {
            // (แก้) เปิดหน้าต่าง AddEditAddressWindow (โหมดเพิ่ม)
            AddEditAddressWindow addWindow = new AddEditAddressWindow();

            // (แก้) ShowDialog() จะ "หยุดรอ" จนกว่าหน้าต่างจะถูกปิด
            // ถ้า User กด "บันทึก" (DialogResult = true) ให้โหลดที่อยู่ใหม่
            if (addWindow.ShowDialog() == true)
            {
                LoadAddresses(); // รีเฟรชรายการที่อยู่
            }
        }

        // (แก้) 👈 2. แก้ไขปุ่ม "แก้ไข"
        private void btnEditAddress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is UserAddress addressToEdit)
            {
                // (แก้) เปิดหน้าต่าง AddEditAddressWindow (โหมดแก้ไข)
                // โดยส่ง Object ที่อยู่ ที่ต้องการแก้ไข เข้าไป
                AddEditAddressWindow editWindow = new AddEditAddressWindow(addressToEdit);

                if (editWindow.ShowDialog() == true)
                {
                    LoadAddresses(); // รีเฟรชรายการที่อยู่
                }
            }
        }

        // (แก้) 👈 3. แก้ไขปุ่ม "ลบ" (ใช้ MessageBox มาตรฐาน)
        private void btnDeleteAddress_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int addressId)
            {
                // (แก้) ใช้ MessageBox มาตรฐานสำหรับคำถาม Yes/No
                if (MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบที่อยู่นี้?", "ยืนยันการลบ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connectionString))
                        {
                            conn.Open();
                            string sql = "DELETE FROM user_addresses WHERE address_id = @id AND user_id = @userId";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", addressId);
                                cmd.Parameters.AddWithValue("@userId", UserSession.UserId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                        LoadAddresses(); // รีเฟรชรายการที่อยู่
                    }
                    catch (Exception ex)
                    {
                        CustomMessageBoxWindow.Show("ไม่สามารถลบที่อยู่: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
                    }
                }
            }
        }

        // --- (โค้ดที่เหลือเหมือนเดิมทั้งหมด) ---

        // (เมธอดจัดการรูปภาพ - เหมือนเดิม)
        private void LoadProfileImage(string imagePath)
        {
            System.Windows.Shapes.Ellipse ellipseElement = ProfileEllipse;
            try
            {
                if (!string.IsNullOrEmpty(imagePath) && System.IO.File.Exists(imagePath))
                {
                    using (System.IO.FileStream stream = new System.IO.FileStream(imagePath, System.IO.FileMode.Open, System.IO.FileAccess.Read))
                    {
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = stream;
                        bitmap.EndInit();
                        ProfileImage.ImageSource = bitmap;
                        ellipseElement.Fill = ProfileImage;
                    }
                }
                else
                {
                    ProfileImage.ImageSource = null;
                    ellipseElement.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C4C4C"));
                }
            }
            catch (Exception)
            {
                ProfileImage.ImageSource = null;
                ellipseElement.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4C4C4C"));
            }
        }

        private void ChangeProfileImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog op = new OpenFileDialog();
            op.Title = "เลือกรูปโปรไฟล์";
            op.Filter = "Image Files (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (op.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = op.FileName;
                    string fileName = $"user_{UserSession.UserId}_{System.IO.Path.GetFileName(sourceFilePath)}";

                    string userImagesDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "UserImages");
                    if (!System.IO.Directory.Exists(userImagesDir))
                    {
                        System.IO.Directory.CreateDirectory(userImagesDir);
                    }
                    string destinationFilePath = System.IO.Path.Combine(userImagesDir, fileName);

                    System.IO.File.Copy(sourceFilePath, destinationFilePath, true);

                    UpdateProfileImagePath(destinationFilePath);
                    LoadProfileImage(destinationFilePath);

                    CustomMessageBoxWindow.Show("เปลี่ยนรูปโปรไฟล์สำเร็จ!", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการเปลี่ยนรูปโปรไฟล์: " + ex.Message, "Image Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        private void UpdateProfileImagePath(string newPath)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "UPDATE users SET profile_image_path = @path WHERE id = @userId";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@path", newPath);
                        cmd.Parameters.AddWithValue("@userId", UserSession.UserId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถบันทึก Path รูปโปรไฟล์ลงฐานข้อมูล: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (เมธอด GetStringSafe - เหมือนเดิม)
        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex))
            {
                return string.Empty;
            }
            return reader.GetString(colIndex);
        }

        // (เมธอดควบคุมหน้าต่าง และ Top Nav - เหมือนเดิม)
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
            if (e.Source == sender) { Window.GetWindow(this)?.DragMove(); }
        }
        private void Logo_Click(object sender, MouseButtonEventArgs e)
        {
            btnShop_Click(sender, new RoutedEventArgs());
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
                SlideManage.Instance.NavigateWithSlide(new WishlistPage(parent), false);
        }

        private void btnAboutUs_Click(object sender, RoutedEventArgs e)
        {
            if (SlideManage.Instance != null)
            {
                // (เรียกใช้หน้าที่เราเพิ่งสร้าง)
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