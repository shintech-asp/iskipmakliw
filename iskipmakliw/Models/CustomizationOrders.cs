namespace iskipmakliw.Models
{
    public class CustomizationOrders
    {
        public int Id { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int SellersId { get; set; }
        public Users Sellers { get; set; }
        public string Model { get; set; }
        public string Color { get; set; }
        public string Texture { get; set; }
        public string Scale { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string SellerStatus { get; set; }
        public decimal? Price { get; set; }
        public string? ModeOfPayment { get; set; }
        public string? PaymentStatus { get; set; }
        public string? TransactionStatus { get; set; }
        public string? CancellationReason { get; set; }
        public ICollection<CustomizationChat> CustomizationChat { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
    }
}
