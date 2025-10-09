using iskipmakliw.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace iskipmakliw.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Users> Users { get; set; }
        public DbSet<UserDetails> UserDetails { get; set; }
        public DbSet<Billings> Billings { get; set; }
        public DbSet<Plans> Plans { get; set; }
        public DbSet<Payments> Payments { get; set; }
        public DbSet<Product> Product { get; set; }
        public DbSet<ProductVariants> ProductVariants { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<Cart> Cart { get; set; }
        public DbSet<PaymentMethod> PaymentMethod { get; set; }
        public DbSet<PurchasedProduct> PurchasedProduct { get; set; }
        public DbSet<DeliverProduct> DeliverProduct { get; set; }
        public DbSet<Ratings> Ratings { get; set; }
        public void UpdateExpiredSubscriptionsOnStartup()
        {
            var basicPlan = Plans.FirstOrDefault(p => p.PlanName == "Basic");
            if (basicPlan == null) return;

            // Grab all expired subs (Current or Renewed) grouped by user
            var expiredByUser = Subscription
                .Where(s => (s.Status == "Current" || s.Status == "Renewed")
                            && s.Expiration.HasValue
                            && s.Expiration.Value < DateTime.Now)
                .GroupBy(s => s.UsersId)
                .ToList();

            // Find users who already have a Current Basic (don’t add another)
            var existingCurrentBasics = Subscription
                .Where(s => s.PlansId == basicPlan.Id && s.Status == "Current")
                .Select(s => s.UsersId)
                .ToHashSet();

            foreach (var group in expiredByUser)
            {
                int userId = group.Key;

                // Mark all expired subs for this user as Expired
                foreach (var sub in group)
                {
                    sub.Status = "Expired";
                }

                // Add back Basic only if they don’t already have a Current one
                if (!existingCurrentBasics.Contains(userId))
                {
                    Subscription.Add(new Subscription
                    {
                        UsersId = userId,
                        PlansId = basicPlan.Id,
                        Status = "Current",
                        Expiration = null,
                        CreatedAt = DateTime.Now
                    });

                    existingCurrentBasics.Add(userId); // prevent duplicate adds in same run
                }
            }
        }
    }
}
