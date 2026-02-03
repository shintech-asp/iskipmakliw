namespace iskipmakliw.Models
{
    public class ProductModel
    {
        public int Id { get; set; }
        public string? ModelName { get; set; }
        public string ImagePath { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public bool isActive { get; set; }
    }
}
