namespace iskipmakliw.Models
{
    public class PaymentMethod
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public string HolderName { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
    }
}
