using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class TripFormViewModel
    {
        public int TripId { get; set; }

        [Required(ErrorMessage = "Vehicle select karo")]
        [Display(Name = "Vehicle")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Driver select karo")]
        [Display(Name = "Driver")]
        public int DriverId { get; set; }

        [Required(ErrorMessage = "Origin city required")]
        [StringLength(100)]
        [Display(Name = "Origin City")]
        public string OriginCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Destination city required")]
        [StringLength(100)]
        [Display(Name = "Destination City")]
        public string DestinationCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Planned start required")]
        [Display(Name = "Planned Start")]
        [DataType(DataType.DateTime)]
        public DateTime? PlannedStart { get; set; }

        [Required(ErrorMessage = "Planned end required")]
        [Display(Name = "Planned End")]
        [DataType(DataType.DateTime)]
        public DateTime? PlannedEnd { get; set; }

        [Display(Name = "Expected Distance (km)")]
        [Range(0, 5000)]
        public decimal? DistanceKm { get; set; }

        [Display(Name = "Expected Revenue (₹)")]
        [Range(0, 10000000)]
        public decimal? Revenue { get; set; }

        [Display(Name = "Expected Fuel Cost (₹)")]
        [Range(0, 1000000)]
        public decimal? FuelCost { get; set; }

        // Dropdowns
        public List<TripVehicleOption> AvailableVehicles { get; set; } = new();
        public List<TripDriverOption> AvailableDrivers { get; set; } = new();

        public List<string> HaryanaCities { get; set; } = new()
        {
            "Gurugram", "Faridabad", "Panipat", "Karnal", "Hisar", "Rohtak",
            "Sonipat", "Ambala", "Yamunanagar", "Rewari", "Sirsa", "Bhiwani",
            "Palwal", "Jind", "Kaithal", "Kurukshetra", "Fatehabad", "Narnaul",
            "Delhi", "Noida", "Chandigarh", "Jaipur", "Ludhiana", "Dehradun"
        };
    }

    public class TripVehicleOption
    {
        public int VehicleId { get; set; }
        public string Label { get; set; } = string.Empty;
    }

    public class TripDriverOption
    {
        public int DriverId { get; set; }
        public string Label { get; set; } = string.Empty;
    }
}