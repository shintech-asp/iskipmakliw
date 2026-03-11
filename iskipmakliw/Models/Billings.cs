namespace iskipmakliw.Models
{
    public class Billings
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ContactNumber { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Country { get; set; }
        public string Zip { get; set; }
        public string LandMark { get; set; }
        public string Longitude { get; set; }
        public string Latitude { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
        public bool? isDeleted { get; set; } = false;
    }
}
