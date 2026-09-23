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
        public DbSet<Department> Departments { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var seedDate = new DateTime(2026, 1, 1, 12, 0, 0);

            // ---------- Defaults ----------
            modelBuilder.Entity<Role>()
                .Property(r => r.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<User>()
                .Property(u => u.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            modelBuilder.Entity<RefreshToken>()
                .Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");

            // Department delete hone pe users' DepartmentId NULL ho jaye
            modelBuilder.Entity<User>()
                .HasOne(u => u.Department)
                .WithMany(d => d.Users)
                .HasForeignKey(u => u.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);    

            // ---------- Seed Roles ----------
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "User",       Description = "Standard application user",                CreatedAt = seedDate },
                new Role { RoleId = 2, RoleName = "Manager",    Description = "Team lead / module manager",               CreatedAt = seedDate },
                new Role { RoleId = 3, RoleName = "Admin",      Description = "Administrator with elevated privileges",   CreatedAt = seedDate },
                new Role { RoleId = 5, RoleName = "HOD",        Description = "Full system access - specific department", CreatedAt = seedDate }
            );

            // ---------- Seed Users ----------
            modelBuilder.Entity<User>().HasData(
                new User { UserId = 2, Username = "admin",      Email = "admin@erp.com",      Password = "admin123",   FullName = "Admin User",    PhoneNumber = "9999999992", RoleId = 3, IsActive = true, CreatedAt = seedDate },
                new User { UserId = 3, Username = "manager",    Email = "manager@erp.com",    Password = "manager123", FullName = "Team Manager",  PhoneNumber = "9999999995", RoleId = 2, IsActive = true, CreatedAt = seedDate },
                new User { UserId = 4, Username = "john",       Email = "john@erp.com",       Password = "john123",    FullName = "John Doe",      PhoneNumber = "9999999993", RoleId = 1, IsActive = true, CreatedAt = seedDate }
            );
        }

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
                if (entry.State == EntityState.Added && entry.Properties.Any(p => p.Metadata.Name == "CreatedAt"))
                    entry.Property("CreatedAt").CurrentValue = DateTime.Now;

                if (entry.Properties.Any(p => p.Metadata.Name == "UpdatedAt"))
                    entry.Property("UpdatedAt").CurrentValue = DateTime.Now;
            }
        }
    }
}