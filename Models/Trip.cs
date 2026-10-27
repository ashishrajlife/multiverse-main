using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class Trip
    {
        [Key]
        public int TripId { get; set; }

        [Required, StringLength(30)]
        public string TripNumber { get; set; } = string.Empty;   // TRP-20260127-001

        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        public int DriverId { get; set; }
        [ForeignKey("DriverId")]
        public Driver? Driver { get; set; }

        [StringLength(100)]
        public string? OriginCity { get; set; }
        [StringLength(100)]
        public string? DestinationCity { get; set; }

        public DateTime? PlannedStart { get; set; }
        public DateTime? ActualStart { get; set; }
        public DateTime? PlannedEnd { get; set; }
        public DateTime? ActualEnd { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? DistanceKm { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? Revenue { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? FuelCost { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Planned";   // Planned | InProgress | Completed | Delayed | Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        // Existing Trip mein add karo:
        public int? StartOdometerKm { get; set; }        // trip start pe odometer
        public int? EndOdometerKm { get; set; }    
        [Column(TypeName = "decimal(10,2)")]      // trip end pe
        public decimal? GpsDistanceKm { get; set; }      // GPS se calculate
        public int? GpsIdleMinutes { get; set; }         // kitne time engine chalu but stationary
        public int? GpsMaxSpeedKmph { get; set; }        // trip mein max speed
        public string? StartAddress { get; set; }        // geocoded start location
        public string? EndAddress { get; set; }
    
    
    
    }
}