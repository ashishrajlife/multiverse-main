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

        public decimal? DistanceKm { get; set; }
        public decimal? Revenue { get; set; }
        public decimal? FuelCost { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Planned";   // Planned | InProgress | Completed | Delayed | Cancelled

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}