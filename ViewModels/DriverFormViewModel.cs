using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class DriverFormViewModel
    {
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Full name required")]
        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number required")]
        [StringLength(20)]
        [Display(Name = "Phone Number")]
        [RegularExpression(@"^[6-9]\d{9}$", ErrorMessage = "10-digit Indian mobile (6-9 se shuru)")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "License number required")]
        [StringLength(20)]
        [Display(Name = "Driving License Number")]
        public string LicenseNumber { get; set; } = string.Empty;

        [Display(Name = "License Expiry")]
        [DataType(DataType.Date)]
        public DateTime? LicenseExpiry { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Available";

        [Display(Name = "Assigned Vehicle")]
        public int? VehicleId { get; set; }

        [Display(Name = "Depot Location")]
        public string? DepotLocation { get; set; }

        [Display(Name = "Linked User Account")]
        public int? UserId { get; set; }

        // Dropdowns
        public List<string> StatusOptions { get; set; } = new()
        {
            "Available", "OnTrip", "OffDuty", "Leave"
        };

        public List<string> HaryanaDepots { get; set; } = new()
        {
            "Gurugram", "Faridabad", "Panipat", "Karnal", "Hisar",
            "Rohtak", "Sonipat", "Ambala", "Yamunanagar", "Rewari"
        };

        public List<VehicleOption> AvailableVehicles { get; set; } = new();
        public List<UserOption> AvailableUsers { get; set; } = new();
    }

    public class VehicleOption
    {
        public int VehicleId { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class UserOption
    {
        public int UserId { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}