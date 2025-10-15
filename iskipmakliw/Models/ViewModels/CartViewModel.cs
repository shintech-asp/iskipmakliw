namespace iskipmakliw.Models.ViewModels
{
    public class CartViewModel
    {
        public Dictionary<string, List<Cart>>? Cart { get; set; }
        public List<PaymentMethod>? PaymentMethod { get; set; }
        public List<Billings>? Billings { get; set; }
    }
}
