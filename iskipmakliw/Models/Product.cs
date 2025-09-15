namespace iskipmakliw.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Details { get; set; }
        public ICollection<ProductVariants>? ProductVariants { get; set; }
        public ICollection<Gallery>? Gallery { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
    }
}
