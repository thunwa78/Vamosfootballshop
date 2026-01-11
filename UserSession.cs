using System;

namespace login_store
{
    // คลาส static นี้จะเก็บข้อมูลผู้ใช้ที่ล็อกอินอยู่
    // เราสามารถเรียกใช้ข้อมูลนี้ได้จากทุกหน้า
    public static class UserSession
    {
        public static int UserId { get; private set; }
        public static string Username { get; private set; }
        public static string Role { get; private set; }

        // เมธอดสำหรับ "เริ่มเซสชัน" ตอนล็อกอิน
        public static void StartSession(int userId, string username, string role)
        {
            UserId = userId;
            Username = username;
            Role = role;
        }

        // เมธอดสำหรับ "จบเซสชัน" ตอน Logout
        public static void EndSession()
        {
            UserId = 0;
            Username = null;
            Role = null;
        }
    }
}