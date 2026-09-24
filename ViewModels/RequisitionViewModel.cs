using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.ViewModels
{
    // ============ LIST VIEW ============
    public class RequisitionListViewModel
    {
        public List<RequisitionRow> Requisitions { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }
        public string? PriorityFilter { get; set; }
        public int TotalRequisitions { get; set; }
        public int DraftCount { get; set; }
        public int PendingCount { get; set; }
        public int ApprovedCount { get; set; }
        public int RejectedCount { get; set; }
        public int PendingAdminCount { get; set; }
    }

    public class RequisitionRow
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? RequestedByName { get; set; }
        public string? DepartmentName { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime RequiredByDate { get; set; }
        public decimal TotalEstimatedAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public int ItemCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    // ============ CREATE / EDIT FORM ============
    public class RequisitionFormViewModel
    {
        public int RequisitionId { get; set; }

        [Required(ErrorMessage = "Title required hai")]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Priority required hai")]
        public string Priority { get; set; } = "Medium";

        [Required(ErrorMessage = "Required By Date required hai")]
        [DataType(DataType.Date)]
        public DateTime RequiredByDate { get; set; } = DateTime.Now.AddDays(7);

        public int? DepartmentId { get; set; }

        public List<RequisitionItemForm> Items { get; set; } = new();

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalEstimatedAmount { get; set; }
    }

    public class RequisitionItemForm
    {
        public int RequisitionItemId { get; set; }

        [Required(ErrorMessage = "Item name required hai")]
        [StringLength(200)]
        public string ItemName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        public string? Category { get; set; }

        [Required]
        [Range(0.001, double.MaxValue, ErrorMessage = "Quantity must be greater than 0")]
        public decimal Quantity { get; set; } = 1;

        [Required]
        public string Unit { get; set; } = "Piece";

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Rate must be 0 or more")]
        public decimal EstimatedRate { get; set; }

        public decimal EstimatedAmount { get; set; }

        public string? Remarks { get; set; }
    }

    // ============ DETAILS VIEW ============
    public class RequisitionDetailsViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? RequestedByName { get; set; }
        public string? RequestedByEmail { get; set; }
        public string? DepartmentName { get; set; }
        public string Priority { get; set; } = string.Empty;
        public DateTime RequiredByDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public decimal TotalEstimatedAmount { get; set; }
        public string? HodRemarks { get; set; }
        public string? RejectionReason { get; set; }
        public DateTime? HodApprovedAt { get; set; }
        public string? HodApprovedByName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public List<RequisitionItemForm> Items { get; set; } = new();

        // Permissions (for view rendering)
        public bool CanEdit { get; set; }
        public bool CanSubmit { get; set; }
        public bool CanApprove { get; set; }
        public bool CanReject { get; set; }
        public bool CanDelete { get; set; }

        public string? AdminApprovedByName { get; set; }
public DateTime? AdminApprovedAt { get; set; }
public string? AdminRemarks { get; set; }
    }

    // ============ HOD APPROVAL FORM ============
    public class RequisitionApprovalViewModel
    {
        public int RequisitionId { get; set; }
        public string RequisitionNumber { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Remarks { get; set; }
        public string Action { get; set; } = "Approve";   // Approve / Reject / Return
        public string? Reason { get; set; }
    }
}