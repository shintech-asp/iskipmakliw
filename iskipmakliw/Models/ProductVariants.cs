using System.ComponentModel.DataAnnotations.Schema;

namespace iskipmakliw.Models
{
    public class ProductVariants
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public string? Material { get; set; }
        public string Dimension { get; set; }
        public string? Color { get; set; }
        public double Price { get; set; }
        public int Quantity { get; set; }
        public int? Discount { get; set; }
        public string? ProductImage { get; set; }
        public ICollection<Cart> Carts { get; set; }
        public ICollection<PurchasedProduct> PurchasedProduct { get; set; }
        public DateTime? isArchive { get; set; }
    }
}
