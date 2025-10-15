namespace iskipmakliw.Models
{
    public class Plans
    {
        public int Id { get; set; }
        public string PlanName { get; set; }
        public string PlanDetails { get; set; }
        public double? Price { get; set; }
        public int? Discount { get; set; }
        public int? DateArchive { get; set; }
        public ICollection<UserDetails>? UserDetails { get; set; }
    }
}
