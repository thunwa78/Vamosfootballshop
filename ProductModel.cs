namespace login_store
{
    public class ProductModel
    {
        public int ModelId { get; set; }
        public string Name { get; set; }
        public int BrandId { get; set; } // 👈 ต้องมี BrandId เพื่อใช้กรอง
    }
}