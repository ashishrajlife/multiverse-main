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
        [Column(TypeName = "decimal(10,2)")]
        public decimal Liters { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal Amount { get; set; }
        [Column(TypeName = "decimal(10,2)")]
        public decimal? OdometerKm { get; set; }

        [StringLength(100)]
        public string? FuelStation { get; set; }   // Indian Oil, HP, BPCL
    }
}