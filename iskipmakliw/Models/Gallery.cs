using Microsoft.AspNetCore.Mvc;

namespace iskipmakliw.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public string ImageType { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
