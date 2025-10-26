namespace iskipmakliw.Models
{
    public class PurchasedProduct
    {
        public int Id { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int? ProductVariantsId { get; set; }
        public ProductVariants? ProductVariants { get; set; }
        public int? CustomizationOrdersId { get; set; }
        public CustomizationOrders? CustomizationOrders { get; set; }
        public string Source { get; set; }
        public int Quantity { get; set; }
        public double? Price { get; set; }
        public string PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PurchasedDate { get; set; } = DateTime.Now;
        public string TransactionStatus { get; set; } = "Pending";
        public int BillingsId { get; set; }
        public Billings Billings { get; set; }
        public List<DeliverProduct> DeliverProduct { get; set; }

    }
}
