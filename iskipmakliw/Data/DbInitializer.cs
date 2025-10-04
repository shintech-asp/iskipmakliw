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
                    DateCreated = DateTime.Now,
                    ContactNumber = "09123456789",
                    Role = "Admin"
                };

                // Hash password at runtime
                admin.Password = hasher.HashPassword(admin, "12345678");

                context.Users.Add(admin);
                context.SaveChanges();
            }
            if (!context.Plans.Any(u => u.PlanName == "Basic"
                         || u.PlanName == "Monthly"
                         || u.PlanName == "Annual"))
            {
                var plans = new List<Plans>
                    {
                        new Plans
                        {
                            PlanName = "Basic",
                            PlanDetails = "For individuals starting out",
                            Price = 0,
                            Discount = 0
                        },
                        new Plans
                        {
                            PlanName = "Monthly",
                            PlanDetails = "Monthly subscription that includes 3d modeling and full access of our features.",
                            Price = 1999,
                            Discount = 10
                        },
                        new Plans
                        {
                            PlanName = "Annual",
                            PlanDetails = "Annual subscription that includes 3d modeling and full access of our features.",
                            Price = 20999,
                            Discount = 20
                        }
                    };

                context.Plans.AddRange(plans);
                context.SaveChanges();
            }
        }

        }
    }
