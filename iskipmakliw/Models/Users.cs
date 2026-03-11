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
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public DateTime? DateArchived { get; set; }
        public DateTime? DateModified { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public string VerificationCode { get; set; }
        public DateTime? CodeCreatedAt { get; set; }
        public DateTime? LastCodeSentAt { get; set; }
        public ICollection<Billings>? Billings { get; set; }
        public ICollection<Payments>? Payments { get; set; }
        public ICollection<Product>? Product { get; set; }
        public ICollection<Subscription>? Subscription { get; set; }
        public ICollection<PurchasedProduct>? PurchasedProduct { get; set; }
        public ICollection<PaymentMethod>? PaymentMethod { get; set; }
        public ICollection<Cart> Carts { get; set; }

        // 🔹 Add 1-to-1 relationship with UserDetails
        public UserDetails? UserDetails { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiry { get; set; }
    }

}
