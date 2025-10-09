namespace iskipmakliw.Models
{
    public class Ratings
    {
        public int Id { get; set; }
        public int ProductVariantsId { get; set; }
        public ProductVariants ProductVariants { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int Stars { get; set; }
        public string? Review { get; set; }
        public string? Image { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PurchasedProductId { get; set; }
        public PurchasedProduct PurchasedProduct { get; set; }
    }
}
