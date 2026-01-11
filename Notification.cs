using System;

namespace login_store
{
    public class Notification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }

        // (เพิ่ม) Property สำหรับแสดงผลใน UI
        public string Timestamp => CreatedAt.ToString("dd MMM yyyy, HH:mm");
        public string StatusText => IsRead ? "อ่านแล้ว" : "ยังไม่อ่าน";
        public System.Windows.FontWeight StatusWeight => IsRead ? System.Windows.FontWeights.Normal : System.Windows.FontWeights.Bold;
        public System.Windows.Media.Brush StatusForeground => IsRead ? System.Windows.Media.Brushes.Gray : System.Windows.Media.Brushes.White;
    }
}