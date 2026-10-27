using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class FuelLog
    {
        [Key]
        public int FuelLogId { get; set; }

        public int VehicleId { get; set; }
        [ForeignKey("VehicleId")]
        public Vehicle? Vehicle { get; set; }

        public DateTime FilledAt { get; set; } = DateTime.Now;

        public decimal Liters { get; set; }
        public decimal Amount { get; set; }
        public decimal? OdometerKm { get; set; }

        [StringLength(100)]
        public string? FuelStation { get; set; }   // Indian Oil, HP, BPCL
    }
}