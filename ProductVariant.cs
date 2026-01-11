using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace login_store
{
    public class ProductVariant
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; } // (ยืนยัน) 1. ต้องมี
        public string SizeName { get; set; }
        public int StockQuantity { get; set; } // (ยืนยัน) 2. สต็อกย่อย

        public int SortOrder { get; set; }
    }
}