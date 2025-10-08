namespace iskipmakliw.Models
{
    public class DeliverProduct
    {
        public int Id { get; set; }
        public int? RiderId { get; set; }
        public int PurchasedProductId { get; set; }
        public PurchasedProduct PurchasedProduct { get; set; }
        public string Status { get; set; }
    }
}
