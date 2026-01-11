namespace login_store
{
    // นี่คือ Model (พิมพ์เขียว) สำหรับตาราง user_addresses
    public class UserAddress
    {
        public int AddressId { get; set; }
        public int UserId { get; set; }
        public string AddressLabel { get; set; } // เช่น "บ้าน", "ที่ทำงาน"
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; } // บ้านเลขที่, ถนน
        public string SubDistrict { get; set; } // ตำบล
        public string District { get; set; } // อำเภอ
        public string Province { get; set; } // จังหวัด
        public string PostalCode { get; set; } // รหัสไปรษณีย์
        public bool IsDefault { get; set; } // true = ที่อยู่หลัก
    }
}