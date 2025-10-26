namespace iskipmakliw.Models
{
    public class VehicleImages
    {
        public int Id { get; set; }
        public string ImagePath { get; set; }
        public int UserDetailsId { get; set; }
        public UserDetails UserDetails { get; set; }
    }
}
