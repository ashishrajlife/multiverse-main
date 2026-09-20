using System.ComponentModel.DataAnnotations;

namespace ERPDemo.Models
{
    public class Organization
    {
        [Key]
        public int OrganizationId { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string Code { get; set; } = string.Empty;   // e.g. ACME-001

        [EmailAddress]
        [StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [StringLength(50)]
        public string Plan { get; set; } = "Basic";       // Basic, Pro, Enterprise

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        [StringLength(15)]
public string? GSTIN { get; set; }

[StringLength(200)]
public string? LegalName { get; set; }   // Auto-filled from GST API

[StringLength(200)]
public string? TradeName { get; set; }   // Auto-filled

[StringLength(50)]
public string? GSTStatus { get; set; }   // Active / Cancelled / Suspended

public DateTime? GSTLastVerifiedAt { get; set; }

        // Navigation
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}