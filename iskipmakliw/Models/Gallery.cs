using Microsoft.AspNetCore.Mvc;

namespace iskipmakliw.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public string ImageType { get; set; }
        public int pUsersId { get; set; }
        // FK to Product
        public int ProductId { get; set; }
        public Product Product { get; set; }
        public int? ProductVariantsId { get; set; }
        public ProductVariants ProductVariants { get; set; }
    }
}
