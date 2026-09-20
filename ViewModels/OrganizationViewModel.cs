using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class OrganizationViewModel
    {
        public List<OrganizationRow> Organizations { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? StatusFilter { get; set; }   // all | active | inactive

        public int TotalOrganizations { get; set; }
        public int ActiveOrganizations { get; set; }
        public int TotalUsersInOrgs { get; set; }
    }

    public class OrganizationRow
    {
        public int OrganizationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string Plan { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public int UserCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateOrganizationViewModel
    {
        [Required, StringLength(150)]
        public string Name { get; set; } = string.Empty;

        [Required, StringLength(20)]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [EmailAddress, StringLength(150)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [StringLength(250)]
        public string? Address { get; set; }

        [StringLength(100)]
        public string? City { get; set; }

        [StringLength(100)]
        public string? Country { get; set; }

        [Required, StringLength(50)]
        public string Plan { get; set; } = "Basic";
    }

    public class EditOrganizationViewModel : CreateOrganizationViewModel
    {
        public int OrganizationId { get; set; }
        public bool IsActive { get; set; }
    }
}