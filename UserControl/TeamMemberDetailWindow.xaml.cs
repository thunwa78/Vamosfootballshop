using System.Windows;
using System.Windows.Input;

namespace login_store
{
    public partial class TeamMemberDetailWindow : Window
    {
        // (Constructor นี้จะรับ Model 'TeamMember' มาจาก AboutUsPage)
        public TeamMemberDetailWindow(TeamMember member)
        {
            InitializeComponent();

            // (เอาข้อมูลจาก Model 'TeamMember' มาแสดง)
            imgMember.Source = member.ImagePreview;
            txtMemberName.Text = member.Name;
            txtMemberRole.Text = member.Role;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close(); // ปิดหน้าต่าง
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove(); // ลากหน้าต่างได้
        }
    }
}