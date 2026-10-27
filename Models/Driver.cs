using System.ComponentModel.DataAnnotations;

namespace ERPDemo.Models
{
    public class Driver
    {
        [Key]
        public int DriverId { get; set; }

        [Required, StringLength(150)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(20)]
        public string? LicenseNumber { get; set; }

        public DateTime? LicenseExpiry { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Available";  // Available | OnTrip | OffDuty | Leave

        public int? VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public int? UserId { get; set; }   // link to User login
        public User? User { get; set; }

        public string? DepotLocation { get; set; }   // Gurugram | Faridabad | Panipat
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}