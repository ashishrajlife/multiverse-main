using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class TeamUserFormViewModel
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required, StringLength(150), EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone")]
        public string? PhoneNumber { get; set; }

        [Required]
        [Display(Name = "Role")]
        public int RoleId { get; set; }

        [Display(Name = "Department")]
        public int? DepartmentId { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}