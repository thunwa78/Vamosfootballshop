using MySqlConnector;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace login_store
{
    public partial class AdminManageAboutUsPage : Page
    {
        private string connectionString = "server=localhost;port=3306;user=root;password=;database=vamos_shop_db;";

        private ObservableCollection<TeamMember> teamMembers = new ObservableCollection<TeamMember>();

        public AdminManageAboutUsPage()
        {
            InitializeComponent();
            TeamListBox.ItemsSource = teamMembers;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadCompanyInfo();
            LoadTeamMembers();
        }

        // --- 1. ตัวช่วย (Helpers) ---

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

        private (string OriginalPath, string RelativePath) BrowseAndCopyImage(string subFolder)
        {
            OpenFileDialog openDialog = new OpenFileDialog();
            openDialog.Filter = "Image Files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openDialog.ShowDialog() == true)
            {
                try
                {
                    string sourceFilePath = openDialog.FileName;
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(sourceFilePath);
                    string destinationFolder = Path.Combine(AppContext.BaseDirectory, "Images", subFolder);

                    if (!Directory.Exists(destinationFolder))
                    {
                        Directory.CreateDirectory(destinationFolder);
                    }

                    string destinationFilePath = Path.Combine(destinationFolder, uniqueFileName);
                    File.Copy(sourceFilePath, destinationFilePath, true);

                    string relativePath = $"/Images/{subFolder}/{uniqueFileName}";
                    return (sourceFilePath, relativePath);
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการคัดลอกไฟล์รูปภาพ: " + ex.Message, "File Error");
                    return (null, null);
                }
            }
            return (null, null);
        }

        private string GetStringSafe(MySqlDataReader reader, string columnName)
        {
            int colIndex = reader.GetOrdinal(columnName);
            if (reader.IsDBNull(colIndex)) return string.Empty;
            return reader.GetString(colIndex);
        }

        // --- 2. ส่วนข้อมูลหลัก (Company Info) ---

        private void LoadCompanyInfo()
        {
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
                            txtDescription.Text = GetStringSafe(reader, "description");
                            txtEmail.Text = GetStringSafe(reader, "email");
                            txtPhone.Text = GetStringSafe(reader, "phone");
                            // (ดึงข้อมูล Tax ID)
                            txtTaxId.Text = GetStringSafe(reader, "tax_id");
                            txtAddress.Text = GetStringSafe(reader, "address");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลหลัก: " + ex.Message, "Database Error");
            }
        }

        private void btnSaveCompanyInfo_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    // (บันทึก Tax ID)
                    string sql = @"
                        INSERT INTO company_info (id, description, email, phone, tax_id, address)
                        VALUES (1, @desc, @email, @phone, @tax, @address)
                        ON DUPLICATE KEY UPDATE
                        description = @desc, email = @email, phone = @phone, tax_id = @tax, address = @address";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@desc", txtDescription.Text.Trim());
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@phone", txtPhone.Text.Trim());
                        cmd.Parameters.AddWithValue("@tax", txtTaxId.Text.Trim()); // (เพิ่ม)
                        cmd.Parameters.AddWithValue("@address", txtAddress.Text.Trim());
                        cmd.ExecuteNonQuery();
                    }
                }
                CustomMessageBoxWindow.Show("บันทึกข้อมูลหลักสำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึกข้อมูลหลัก: " + ex.Message, "Database Error");
            }
        }

        // --- 3. ส่วนจัดการทีม (Team Members) ---

        private void LoadTeamMembers()
        {
            teamMembers.Clear();
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    string sql = "SELECT * FROM team_members ORDER BY sort_order ASC, name ASC";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string path = GetStringSafe(reader, "image_path");
                            teamMembers.Add(new TeamMember
                            {
                                MemberId = reader.GetInt32("member_id"),
                                Name = GetStringSafe(reader, "name"),
                                Role = GetStringSafe(reader, "role"),
                                ImagePath = path,
                                SortOrder = reader.GetInt32("sort_order"),
                                ImagePreview = LoadImagePreview(path)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("ไม่สามารถโหลดข้อมูลทีม: " + ex.Message, "Database Error");
            }
        }

        private void btnBrowseMemberImage_Click(object sender, RoutedEventArgs e)
        {
            var (originalPath, relativePath) = BrowseAndCopyImage("Team");

            if (relativePath != null)
            {
                txtMemberImagePath.Text = relativePath;
                imgMemberPreview.Source = LoadImagePreview(relativePath);
            }
        }

        private void btnAddMember_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtMemberName.Text))
            {
                CustomMessageBoxWindow.Show("กรุณากรอกชื่อ-นามสกุล", "ข้อมูลไม่ครบ");
                return;
            }

            int memberId = 0;
            int.TryParse(txtEditingMemberId.Text, out memberId);

            string sql;
            if (memberId == 0)
            {
                sql = "INSERT INTO team_members (name, role, image_path) VALUES (@name, @role, @path)";
            }
            else
            {
                sql = "UPDATE team_members SET name = @name, role = @role, image_path = @path WHERE member_id = @id";
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", memberId);
                        cmd.Parameters.AddWithValue("@name", txtMemberName.Text.Trim());
                        cmd.Parameters.AddWithValue("@role", txtMemberRole.Text.Trim());
                        cmd.Parameters.AddWithValue("@path", txtMemberImagePath.Text);
                        cmd.ExecuteNonQuery();
                    }
                }
                CustomMessageBoxWindow.Show("บันทึกข้อมูลทีมสำเร็จ", "Success", CustomMessageBoxWindow.MessageBoxType.Success);
                LoadTeamMembers();
                ResetTeamForm();
            }
            catch (Exception ex)
            {
                CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการบันทึกข้อมูลทีม: " + ex.Message, "Database Error");
            }
        }

        private void btnEditMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TeamMember member)
            {
                txtEditingMemberId.Text = member.MemberId.ToString();
                txtMemberName.Text = member.Name;
                txtMemberRole.Text = member.Role;
                txtMemberImagePath.Text = member.ImagePath;
                imgMemberPreview.Source = member.ImagePreview;

                btnAddMember.Content = "บันทึกการแก้ไข";
                btnCancelEditMember.Visibility = Visibility.Visible;
            }
        }

        private void btnCancelEditMember_Click(object sender, RoutedEventArgs e)
        {
            ResetTeamForm();
        }

        private void btnDeleteMember_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int memberId)
            {
                if (MessageBox.Show("คุณแน่ใจหรือไม่ว่าต้องการลบสมาชิกทีมคนนี้?", "ยืนยันการลบ", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                {
                    return;
                }

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connectionString))
                    {
                        conn.Open();
                        string sql = "DELETE FROM team_members WHERE member_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", memberId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                    LoadTeamMembers();
                }
                catch (Exception ex)
                {
                    CustomMessageBoxWindow.Show("เกิดข้อผิดพลาดในการลบ: " + ex.Message, "Database Error");
                }
            }
        }

        private void ResetTeamForm()
        {
            txtEditingMemberId.Text = "0";
            txtMemberName.Clear();
            txtMemberRole.Clear();
            txtMemberImagePath.Text = "";
            imgMemberPreview.Source = null;

            btnAddMember.Content = "เพิ่มทีม";
            btnCancelEditMember.Visibility = Visibility.Collapsed;
        }
    }
}