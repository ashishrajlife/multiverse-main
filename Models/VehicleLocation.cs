using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class VehicleLocation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int VehicleId { get; set; }

        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal Latitude { get; set; }

        [Column(TypeName = "decimal(10,7)")]
        public decimal Longitude { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? SpeedKmph { get; set; }

        [Column(TypeName = "decimal(6,2)")]
        public decimal? HeadingDegrees { get; set; }

        public int? Ignition { get; set; }

        [Column(TypeName = "decimal(12,2)")]
        public decimal? OdometerKm { get; set; }

        public DateTime LastUpdatedAt { get; set; }
        public string? Address { get; set; }
        public int? CurrentTripId { get; set; }
    }
}