namespace iskipmakliw.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int ProductVariantsId { get; set; }
        public ProductVariants ProductVariants { get; set; }
        public int Quantity { get; set; }
    }
}
