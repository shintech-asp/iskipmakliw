namespace iskipmakliw.Models.ViewModels
{
    public class ProductDetailsViewModel
    {
        public Product Product { get; set; }
        public List<ProductVariants> ProductVariants { get; set; }
        public ProductVariants ProductDetails { get; set; }
    }
}
