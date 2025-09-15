using Microsoft.AspNetCore.Mvc;

namespace iskipmakliw.Models
{
    public class Gallery
    {
        public int Id { get; set; }
        public byte[] Image { get; set; }
        public string ImageType { get; set; }
        public int pUsersId { get; set; }
        public int? pProductId { get; set; }
    }
}
