namespace iskipmakliw.Models.ViewModels
{
    public class SellersIndexViewModel
    {
        public Users Users { get; set; }
        public List<PurchasedProduct> PurchasedProduct { get; set; }
        public List<ProductVariants> ProductVariants { get; set; }
        public List<DeliverProduct> Sales { get; set; }
    }
}
