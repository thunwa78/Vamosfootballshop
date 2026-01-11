using System;

namespace login_store
{
    // Model สำหรับตาราง `user_vouchers` (โค้ดที่ User เก็บ)
    // (เราจะเพิ่ม Property จากตาราง Voucher เข้ามาด้วย
    // เพื่อให้ใช้งานง่ายในหน้า "โค้ดของฉัน")
    public class UserVoucher
    {
        // จากตาราง user_vouchers
        public int UserVoucherId { get; set; }
        public int UserId { get; set; }
        public int VoucherId { get; set; }
        public DateTime CollectedDate { get; set; }
        public bool IsUsed { get; set; }

        // (ที่ Join มาจากตาราง vouchers)
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal MinPurchase { get; set; }
        public DateTime ValidTo { get; set; }
    }
}