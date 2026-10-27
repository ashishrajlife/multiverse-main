using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class Vehicle
    {
        [Key]
        public int VehicleId { get; set; }

        [Required, StringLength(20)]
        public string RegistrationNumber { get; set; } = string.Empty;  // HR-26-AB-1234

        [Required, StringLength(50)]
        public string VehicleType { get; set; } = string.Empty;        // Truck, Bus, Tempo, Tanker

        [StringLength(50)]
        public string? Make { get; set; }        // Tata, Ashok Leyland, Eicher

        [StringLength(50)]
        public string? Model { get; set; }

        public int? Year { get; set; }

        public decimal? CapacityTons { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "Active";   // Active | Idle | Maintenance | Retired

        [StringLength(50)]
        public string? FuelType { get; set; }             // Diesel, CNG, Electric

        public int? CurrentOdometerKm { get; set; }

        // Haryana compliance
        public DateTime? InsuranceExpiry { get; set; }
        public DateTime? FitnessExpiry { get; set; }
        public DateTime? PermitExpiry { get; set; }
        public DateTime? PucExpiry { get; set; }

        public DateTime? LastServiceDate { get; set; }
        public DateTime? NextServiceDue { get; set; }

        public string? PlantLocation { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }
    }
}