using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class GpsLog
    {
        public long GpsLogId { get; set; }

        public int GpsDeviceId { get; set; }
        public GpsDevice? GpsDevice { get; set; }

        public int VehicleId { get; set; }              // denormalized for fast queries
        public int? TripId { get; set; }                // null = not on trip

        [Column(TypeName = "decimal(10,7)")]
public decimal Latitude { get; set; }

[Column(TypeName = "decimal(10,7)")]
public decimal Longitude { get; set; }

[Column(TypeName = "decimal(6,2)")]
public decimal? SpeedKmph { get; set; }

[Column(TypeName = "decimal(6,2)")]
public decimal? HeadingDegrees { get; set; }

[Column(TypeName = "decimal(12,2)")]
public decimal? OdometerKm { get; set; }

[Column(TypeName = "decimal(5,2)")]
public decimal? FuelLevelPercent { get; set; }
    

        public int? Ignition { get; set; }              // 0/1


        public DateTime RecordedAt { get; set; }        // device ka timestamp
        public DateTime ReceivedAt { get; set; } = DateTime.Now; // server ne kab accept kiya

        [StringLength(20)]
        public string? Source { get; set; }             // webhook | poll | mqtt
    }
}