namespace iskipmakliw.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string ContactNumber { get; set; }
        public string Role { get; set; }
        public string Password { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateArchived { get; set; }
        public DateTime? DateModified { get; set; }
        public ICollection<Gallery>? Gallery { get; set; }
        public ICollection<Billings>? Billings { get; set; }
        public ICollection<Payments>? Payments { get; set; }
    }
}
