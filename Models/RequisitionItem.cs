using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class RequisitionItem
    {
        [Key]
        public int RequisitionItemId { get; set; }

        public int RequisitionId { get; set; }
        [ForeignKey("RequisitionId")]
        public Requisition? Requisition { get; set; }

        [Required, StringLength(200)]
        [Display(Name = "Item Name")]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }   // Raw Material / Spare / Consumable / Service

        [Column(TypeName = "decimal(18,3)")]
        public decimal Quantity { get; set; }

        [Required, StringLength(20)]
        public string Unit { get; set; } = "Piece";   // MT / KG / Piece / Litre

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Rate")]
        public decimal EstimatedRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Estimated Amount")]
        public decimal EstimatedAmount { get; set; }

        [StringLength(500)]
        public string? Remarks { get; set; }
    }
}