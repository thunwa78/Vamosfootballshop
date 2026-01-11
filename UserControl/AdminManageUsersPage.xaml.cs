//using MySql.Data.MySqlClient;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
// (ลบ using System.Windows.Media; ออกได้เลยครับ)

namespace login_store
{
    // (คลาส UserView - เหมือนเดิม)
    public class UserView
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }

        public string FullName
        {
            get
            {
                if (string.IsNullOrEmpty(FirstName) && string.IsNullOrEmpty(LastName)) return "-";
                return $"{FirstName} {LastName}".Trim();
            }
        }
    }

    public partial class AdminManageUsersPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";
        private List<UserView> allUsers = new List<UserView>();

        public AdminManageUsersPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }

        // --- 1. เมธอดหลัก: โหลดข้อมูลลูกค้า ---
        private void LoadUsers(string searchTerm = "")
        {
            allUsers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT id, username, email, role, first_name, last_name, phone_number FROM users WHERE id != @admin_id ";

                    if (!string.IsNullOrEmpty(searchTerm))
                    {
                        sql += "AND (username LIKE @search OR email LIKE @search OR first_name LIKE @search OR last_name LIKE @search) ";
                    }
                    sql += "ORDER BY username ASC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@admin_id", UserSession.UserId);
                        if (!string.IsNullOrEmpty(searchTerm))
                        {
                            cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
                        }

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                allUsers.Add(new UserView
                                {
                                    Id = reader.GetInt32("id"),
                                    Username = reader.GetString("username"),
                                    Email = reader.GetString("email"),
                                    Role = reader.GetString("role"),
                                    FirstName = GetStringSafe(reader, "first_name"),
                                    LastName = GetStringSafe(reader, "last_name"),
                                    Phone = GetStringSafe(reader, "phone_number")
                                });
                            }
                        }
                    }

                    if (UsersDataGrid != null)
                    {
                        UsersDataGrid.ItemsSource = null;
                        UsersDataGrid.ItemsSource = allUsers;
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลลูกค้า: " + ex.Message, "Database Error", CustomMessageBoxWindow.MessageBoxType.Error);
            }
        }

        // (เมธอดช่วย ป้องกัน DBNull)
        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex))
            {
                return string.Empty;
            }
            return reader.GetString(colIndex);
        }

        // --- 2. Event Handlers ---
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers(txtSearch.Text);
        }

        private void txtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadUsers(txtSearch.Text);
            }
        }

        // (แก้) 👈 3. แก้ไขเมธอดนี้ใหม่ทั้งหมด
        private void ViewOrders_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int userId)
            {
                // (แก้) 1. ตรวจสอบว่าหน้านี้มี NavigationService หรือไม่
                if (this.NavigationService != null)
                {
                    // (แก้) 2. สั่งให้ NavigationService เปิดหน้า Orders
                    // (อันนี้จะเวิร์คเพราะเราแก้ AdminManageOrdersPage ให้รับ int แล้ว)
                    this.NavigationService.Navigate(new AdminManageOrdersPage(userId));
                }
                else
                {
                    // (แก้) 3. เปลี่ยนข้อความ Error (ถ้ายังเกิด)
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาด: ไม่พบ NavigationService", "Navigation Error", CustomMessageBoxWindow.MessageBoxType.Error);
                }
            }
        }

        // (ลบ) 👈 4. ลบเมธอด "FindParent" ทิ้ง
    }
}