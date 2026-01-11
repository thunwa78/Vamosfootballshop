using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq; // (เพิ่ม)
using System.Windows.Media; // (เพิ่ม)

namespace login_store
{
    public partial class AdminDashboardPage : Page
    {
        private SlideManage parent;

        public AdminDashboardPage(SlideManage parent)
        {
            InitializeComponent();
            this.parent = parent;
            txtAdminName.Text = $"Admin: {UserSession.Username}";

            // (แก้ไข) โหลดหน้า Dashboard เป็นหน้าแรก
            AdminContentFrame.Navigate(new AdminDashboardContentPage());
            SetActiveButton(btnDashboard); // (แก้ไข) ตั้งค่าปุ่ม Dashboard ให้ Active
        }

        // --- เมธอดควบคุมหน้าต่าง (ต้องเติมโค้ด) ---
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
            if (e.OriginalSource.GetType() == typeof(Border))
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        private void btnDashboard_Click(object sender, RoutedEventArgs e)
        {
            // (แก้ไข) โหลดหน้า Dashboard จริง
            AdminContentFrame.Navigate(new AdminDashboardContentPage());
            SetActiveButton(sender as Button);
        }

        private void btnManageOrders_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageOrdersPage());
            SetActiveButton(sender as Button);
        }

        private void btnManageProducts_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageProductsPage());
            SetActiveButton(sender as Button);
        }

        private void btnManageBrands_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageBrandsPage());
            SetActiveButton(sender as Button);
        }

        private void btnManageCategories_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageCategoriesPage());
            SetActiveButton(sender as Button);
        }

        private void btnManageModels_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageModelsPage());
            SetActiveButton(sender as Button);
        }

        // (เพิ่ม) 👈 เพิ่มเมธอดนี้สำหรับปุ่มใหม่
        private void btnManageVouchers_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageVouchersPage());
            SetActiveButton(sender as Button);
        }

        public void btnManageAds_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageAdsPage()); // (ต้องสร้างหน้า AdminManageAdsPage.xaml.cs)
            SetActiveButton(sender as Button);
        }
        private void btnManageUsers_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new AdminManageUsersPage());
            SetActiveButton(sender as Button);
        }


        // (เพิ่ม) 👈 เมธอดสำหรับปุ่ม "จัดการ 'เกี่ยวกับเรา'"
        private void btnManageAboutUs_Click(object sender, RoutedEventArgs e)
        {
            // (เรียกใช้หน้าที่เราเพิ่งสร้าง)
            AdminContentFrame.Navigate(new AdminManageAboutUsPage());
            SetActiveButton(sender as Button);
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            UserSession.EndSession();
            if (SlideManage.Instance != null)
            {
                SlideManage.Instance.NavigateWithSlide(new LoginPage(parent), true);
            }
        }

        // (เมธอดช่วย) ทำให้ปุ่มที่ถูกเลือกดู Active (ตัวหนา)
        public void SetActiveButton(Button activeButton)
        {
            // รีเซ็ตปุ่มทั้งหมด
            foreach (var child in (btnDashboard.Parent as StackPanel).Children)
            {
                if (child is Button btn)
                {
                    btn.IsEnabled = true;
                    btn.FontWeight = FontWeights.Normal;
                    btn.Background = Brushes.Transparent; // (ทางเลือก: สีปกติ)
                }
            }
            // ตั้งค่าปุ่มที่เพิ่งกด
            if (activeButton != null)
            {
                activeButton.IsEnabled = false;
                activeButton.FontWeight = FontWeights.Bold;
                activeButton.Background = new SolidColorBrush(Color.FromArgb(50, 255, 255, 255)); // (ทางเลือก: สี Active)
            }
        }
    }
}