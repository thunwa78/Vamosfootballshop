using System;
using System.Collections.Generic;
using System.Windows.Media.Imaging; // (ต้องมี)

namespace login_store
{
    public class Product
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImagePath { get; set; }

        public string SizeChartPath { get; set; }

        public bool IsInWishlist { get; set; } = false;
        public int StockQuantity { get; set; }

        public int? BrandId { get; set; }
        public int? CategoryId { get; set; }
        public bool IsActive { get; set; }

        // (ที่เก็บพรีวิวสำหรับ UI)
        public BitmapImage ImagePreview { get; set; }

        public List<ProductImage> GalleryImages { get; set; } = new List<ProductImage>();
        public List<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}