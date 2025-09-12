namespace iskipmakliw.Models
{
    public class Payments
    {
        public int Id { get; set; }
        public double? Amount { get; set; }
        public string PaymentDetails { get; set; }
        public string Status { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public int? SellersId { get; set; }
    }
}
