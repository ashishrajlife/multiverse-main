using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class MaintenanceLog
    {
        [Key]
        public int MaintenanceId { get; set; }

        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        [Required, StringLength(100)]
        public string ServiceType { get; set; } = string.Empty;   // Oil Change, Tyre, Brake

        public DateTime ServiceDate { get; set; } = DateTime.Now;
        public decimal? Cost { get; set; }

        [StringLength(250)]
        public string? Notes { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "Completed";   // Scheduled | InProgress | Completed
    }
}