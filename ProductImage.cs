using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging; // (เพิ่มบรรทัดนี้)

namespace login_store
{
    public class ProductImage
    {
        public int ImageId { get; set; }
        public int ProductId { get; set; }
        public string ImagePath { get; set; } // <== อันนี้สำหรับ Database
        public int SortOrder { get; set; }

        // (เพิ่ม Property นี้)
        public BitmapImage ImagePreview { get; set; } // <== อันนี้สำหรับ UI
    }
}