using System.Windows.Media.Imaging; // 👈 (ต้องมี)

namespace login_store
{
    public class TeamMember
    {
        public int MemberId { get; set; }
        public string Name { get; set; }
        public string Role { get; set; }
        public string ImagePath { get; set; }
        public int SortOrder { get; set; }

        // (สำหรับแสดงพรีวิวใน UI)
        public BitmapImage ImagePreview { get; set; }
    }
}