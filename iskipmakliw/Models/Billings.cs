namespace iskipmakliw.Models
{
    public class Billings
    {
        public int Id { get; set; }
        public string LocationDetails { get; set; }
        public string LandMark { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string ContactPerson { get; set; }
        public string ContactNumber { get; set; }
        public DateTime DateArchive { get; set; }
        public int UsersId { get; set; }
        public Users Users { get; set; }
    }
}
