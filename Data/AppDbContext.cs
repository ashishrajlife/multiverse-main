using ERPDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace ERPDemo.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2026, 1, 1, 12, 0, 0);

            modelBuilder.Entity<Role>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<RefreshToken>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // ✅ Seed Roles with static DateTime
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "User",       Description = "Standard application user",             CreatedAt = seedDate },
                new Role { RoleId = 2, RoleName = "Admin",      Description = "Administrator with elevated privileges", CreatedAt = seedDate },
                new Role { RoleId = 3, RoleName = "SuperAdmin", Description = "Full system access - super administrator", CreatedAt = seedDate }
            );

            // ✅ Seed Users with static DateTime
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    UserId = 1,
                    Username = "superadmin",
                    Email = "superadmin@erp.com",
                    Password = "super123",
                    FullName = "Super Admin",
                    PhoneNumber = "9999999991",
                    RoleId = 3,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new User
                {
                    UserId = 2,
                    Username = "admin",
                    Email = "admin@erp.com",
                    Password = "admin123",
                    FullName = "Admin User",
                    PhoneNumber = "9999999992",
                    RoleId = 2,
                    IsActive = true,
                    CreatedAt = seedDate
                },
                new User
                {
                    UserId = 3,
                    Username = "john",
                    Email = "john@erp.com",
                    Password = "john123",
                    FullName = "John Doe",
                    PhoneNumber = "9999999993",
                    RoleId = 1,
                    IsActive = true,
                    CreatedAt = seedDate
                }
            );
        }

        // ✅ Auto-set CreatedAt / UpdatedAt for runtime inserts
        public override int SaveChanges()
        {
            ApplyTimestamps();
            return base.SaveChanges();
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            ApplyTimestamps();
            return base.SaveChangesAsync(cancellationToken);
        }

        private void ApplyTimestamps()
        {
            var entries = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Properties.Any(p => p.Metadata.Name == "CreatedAt") && entry.State == EntityState.Added)
                {
                    entry.Property("CreatedAt").CurrentValue = DateTime.Now;
                }

                if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
                {
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
                }
            }
        }
    }
}