namespace iskipmakliw.Models
{
    public class DeliverProduct
    {
        public int Id { get; set; }
        public int? RiderId { get; set; }
        public Users Rider { get; set; }
        public int PurchasedProductId { get; set; }
        public PurchasedProduct PurchasedProduct { get; set; }
        public string Status { get; set; }
        public DateTime? PickUpOn { get; set; }
        public DateTime? DropOffOn { get; set; }
        public DateTime? DeliveredOn { get; set; }
        public DateTime? AcceptedOn { get; set; }
        public string? DriversLat { get; set; }
        public string? DriversLong { get; set; }
        public bool isRemitted { get; set; } = false;
        public DateTime? RemittedOn { get; set; }
    }
}
