namespace iskipmakliw.Models
{
    public class CustomizationChat
    {
        public int Id { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int SellersId { get; set; }
        public Users Sellers { get; set; }
        public int CustomizationOrdersId { get; set; }
        public CustomizationOrders CustomizationOrders { get; set; }
        public string Message { get; set; }
        public DateTime DateSent { get; set; } = DateTime.UtcNow.AddHours(8);
        public DateTime? DateReceived { get; set; }
        public bool IsFromBuyer { get; set; }

    }
}
