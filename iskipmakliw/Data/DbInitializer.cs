﻿using iskipmakliw.Models;
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
                         || u.PlanName == "Plus"
                         || u.PlanName == "Business"))
            {
                var plans = new List<Plans>
                    {
                        new Plans
                        {
                            PlanName = "Basic",
                            PlanDetails = "For individuals starting out",
                            Price = 999,
                            Discount = 20
                        },
                        new Plans
                        {
                            PlanName = "Plus",
                            PlanDetails = "For growing users",
                            Price = 1999,
                            Discount = 15
                        },
                        new Plans
                        {
                            PlanName = "Business",
                            PlanDetails = "For teams and enterprises",
                            Price = 4999,
                            Discount = 10
                        }
                    };

                context.Plans.AddRange(plans);
                context.SaveChanges();
            }
        }

        }
    }
