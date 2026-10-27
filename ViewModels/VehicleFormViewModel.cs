using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class VehicleFormViewModel
    {
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Registration number required")]
        [StringLength(20)]
        [Display(Name = "Registration Number")]
        [RegularExpression(@"^[A-Z]{2}[- ]?\d{1,2}[- ]?[A-Z]{1,3}[- ]?\d{4}$",
            ErrorMessage = "Format: HR-26-AB-1234 ya HR26AB1234")]
        public string RegistrationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vehicle type required")]
        [Display(Name = "Vehicle Type")]
        public string VehicleType { get; set; } = "Truck";

        [Display(Name = "Make")]
        public string? Make { get; set; }

        [Display(Name = "Model")]
        public string? Model { get; set; }

        [Range(1980, 2100, ErrorMessage = "Valid year daalo")]
        public int? Year { get; set; }

        [Display(Name = "Capacity (Tons)")]
        [Range(0, 100, ErrorMessage = "0-100 ke beech")]
        public decimal? CapacityTons { get; set; }

        [Required]
        [Display(Name = "Status")]
        public string Status { get; set; } = "Active";

        [Display(Name = "Fuel Type")]
        public string? FuelType { get; set; }

        [Display(Name = "Current Odometer (km)")]
        [Range(0, 5000000)]
        public int? CurrentOdometerKm { get; set; }

        [Display(Name = "Insurance Expiry")]
        [DataType(DataType.Date)]
        public DateTime? InsuranceExpiry { get; set; }

        [Display(Name = "Fitness Expiry")]
        [DataType(DataType.Date)]
        public DateTime? FitnessExpiry { get; set; }

        [Display(Name = "Permit Expiry")]
        [DataType(DataType.Date)]
        public DateTime? PermitExpiry { get; set; }

        [Display(Name = "PUC Expiry")]
        [DataType(DataType.Date)]
        public DateTime? PucExpiry { get; set; }

        [Display(Name = "Last Service Date")]
        [DataType(DataType.Date)]
        public DateTime? LastServiceDate { get; set; }

        [Display(Name = "Next Service Due")]
        [DataType(DataType.Date)]
        public DateTime? NextServiceDue { get; set; }

        [Display(Name = "Plant / Depot Location")]
        public string? PlantLocation { get; set; }

        // Dropdowns
        public List<string> VehicleTypes { get; set; } = new()
        {
            "Truck", "Trailer", "Tempo", "Bus", "Tanker", "LCV", "Pickup"
        };

        public List<string> StatusOptions { get; set; } = new()
        {
            "Active", "Idle", "Maintenance", "Retired"
        };

        public List<string> FuelTypes { get; set; } = new()
        {
            "Diesel", "CNG", "Petrol", "Electric", "LNG"
        };

        public List<string> HaryanaDepots { get; set; } = new()
        {
            "Gurugram", "Faridabad", "Panipat", "Karnal", "Hisar",
            "Rohtak", "Sonipat", "Ambala", "Yamunanagar", "Rewari"
        };
    }
}