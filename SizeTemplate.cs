using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace login_store
{
    public class SizeTemplate
    {
        public int SizeId { get; set; }
        public string SizeName { get; set; }

        // (สำคัญ) เพิ่ม CategoryId เพื่อใช้ใน Logic การ Filter
        public int CategoryId { get; set; }
    }
}
