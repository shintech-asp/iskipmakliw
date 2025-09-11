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
        public DbSet<Gallery> Gallery { get; set; }
        public DbSet<Billings> Billings { get; set; }
        public DbSet<Plans> Plans { get; set; }
        public DbSet<Payments> Payments { get; set; }
    }
}
