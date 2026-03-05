namespace iskipmakliw.Models.DTO
{
    public class ProductVariantDto
    {
        public int Id { get; set; }
        public string Color { get; set; }
        public string Size { get; set; }
        public string? Weight { get; set; }
        public string? Height { get; set; }
        public string? Width { get; set; }
        public string DiscountType { get; set; }
        public double Price { get; set; }
        public int Stock { get; set; }
        public string ProductImage { get; set; }
        public int? Discount { get; set; }
    }
}
