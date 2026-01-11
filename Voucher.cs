using System;

namespace login_store
{
    // Model สำหรับตาราง `vouchers` (โค้ดหลัก)
    public class Voucher
    {
        public int VoucherId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal DiscountAmount { get; set; }

        public string DiscountType { get; set; }
        public decimal MinPurchase { get; set; }
        public DateTime ValidFrom { get; set; }
        public DateTime ValidTo { get; set; }
        public bool IsActive { get; set; }
    
    public string DiscountDisplay
        {
            get
            {
                if (DiscountType == "Percentage")
                {
                    return $"{DiscountAmount:N0} %"; // แสดงเป็น "20 %"
                }
                else
                {
                    return $"{DiscountAmount:N2} บาท"; // แสดงเป็น "100.00 บาท"
                }
            }
        }
    }
}