using iskipmakliw.Models;
using Microsoft.AspNetCore.Identity;
using System.Linq;

namespace iskipmakliw.Data
{
    public static class DbInitializer
    {
        public static void Seed(ApplicationDbContext context)
        {
            // Ensure database is created and latest migrations applied
            context.Database.EnsureCreated();

            // Check if admin already exists
            if (!context.Users.Any(u => u.Email == "admin@test.com"))
            {
                var hasher = new PasswordHasher<Users>();

                var admin = new Users
                {
                    Username = "Administrator",
                    Email = "admin@test.com",
                    DateCreated = DateTime.Now
                };

                // Hash password at runtime
                admin.Password = hasher.HashPassword(admin, "12345678");

                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
