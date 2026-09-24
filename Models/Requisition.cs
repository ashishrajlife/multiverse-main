using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class Requisition
    {
        [Key]
        public int RequisitionId { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Requisition Number")]
        public string RequisitionNumber { get; set; } = string.Empty;   // REQ-2026-0001

        [Required, StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        // Requester
        public int RequestedByUserId { get; set; }
        [ForeignKey("RequestedByUserId")]
        public User? RequestedByUser { get; set; }

        // Department
        public int? DepartmentId { get; set; }
        [ForeignKey("DepartmentId")]
        public Department? Department { get; set; }

        [Required, StringLength(20)]
        public string Priority { get; set; } = "Medium";   // Low / Medium / High / Urgent

        [Required]
        [Display(Name = "Required By Date")]
        [DataType(DataType.Date)]
        public DateTime RequiredByDate { get; set; }

        // Status
        [Required, StringLength(30)]
        public string Status { get; set; } = "Draft";
        // Draft / PendingHOD / ApprovedByHOD / Rejected / Cancelled / Fulfilled

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEstimatedAmount { get; set; }

        // HOD Approval
        public int? HodApprovedByUserId { get; set; }
        [ForeignKey("HodApprovedByUserId")]
        public User? HodApprovedByUser { get; set; }

        public DateTime? HodApprovedAt { get; set; }

        [StringLength(500)]
        public string? HodRemarks { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime? UpdatedAt { get; set; }

        // Navigation
        public ICollection<RequisitionItem> Items { get; set; } = new List<RequisitionItem>();
    }
}