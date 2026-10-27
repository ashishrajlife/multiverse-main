using System.ComponentModel.DataAnnotations;

namespace ERPDemo.Models
{
    public class GpsDevice
    {
        public int GpsDeviceId { get; set; }

        [Required, StringLength(50)]
        public string Imei { get; set; } = string.Empty;      // device ka unique ID

        [Required, StringLength(50)]
        public string DeviceModel { get; set; } = string.Empty; // Teltonika FMB920, Concox...

        [StringLength(50)]
        public string? SimNumber { get; set; }

        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }

        public DateTime? InstalledAt { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Active";   // Active | Inactive | Maintenance

        public DateTime? LastPingAt { get; set; }        // last time device ne data bheja

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}