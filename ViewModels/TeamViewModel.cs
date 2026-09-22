using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class TeamViewModel
    {
        public List<TeamRow> Members { get; set; } = new();
        public string? SearchTerm { get; set; }
        public string? RoleFilter { get; set; }
        public string? StatusFilter { get; set; }

        public int TotalMembers { get; set; }
        public int ActiveMembers { get; set; }
        public int ManagerCount { get; set; }
        public int UserCount { get; set; }
    }

    public class TeamRow
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public bool IsActive { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CreateTeamMemberViewModel
    {
        [Required(ErrorMessage = "Username required hai"), StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email required hai"), EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role select karo")]
        public int RoleId { get; set; }

        public int? DepartmentId { get; set; }

        // Personal Details
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [Display(Name = "City")]
        public string? City { get; set; } = "Raipur"; // Default to Raipur

        [Display(Name = "State")]
        public string? State { get; set; } = "Chhattisgarh"; // Default to CG

        [Display(Name = "Pincode")]
        public string? Pincode { get; set; }

        [Display(Name = "Aadhaar Number")]
        public string? AadhaarNumber { get; set; }

        [Display(Name = "PAN Number")]
        public string? PanNumber { get; set; }

        // Employment Details
        [Display(Name = "Employment Type")]
        public string? EmploymentType { get; set; }

        [Display(Name = "Designation")]
        public string? Designation { get; set; }

        [Display(Name = "Plant Location")]
        public string? PlantLocation { get; set; }

        [Display(Name = "Joining Date")]
        [DataType(DataType.Date)]
        public DateTime? JoiningDate { get; set; }

        [Display(Name = "Shift Start Time")]
        [DataType(DataType.Time)]
        public string? ShiftStartTime { get; set; }

        [Display(Name = "Shift End Time")]
        [DataType(DataType.Time)]
        public string? ShiftEndTime { get; set; }

        [Display(Name = "Salary Type")]
        public string? SalaryType { get; set; }

        [Display(Name = "Salary Amount")]
        public decimal? SalaryAmount { get; set; }

    }

    public class EditTeamMemberViewModel
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }

        [Required]
        public int RoleId { get; set; }

        public bool IsActive { get; set; }
        public int? DepartmentId { get; set; }

        // Personal Details
[Display(Name = "Address")]
public string? Address { get; set; }

[Display(Name = "City")]
public string? City { get; set; } = "Raipur"; // Default to Raipur

[Display(Name = "State")]
public string? State { get; set; } = "Chhattisgarh"; // Default to CG

[Display(Name = "Pincode")]
public string? Pincode { get; set; }

[Display(Name = "Aadhaar Number")]
public string? AadhaarNumber { get; set; }

[Display(Name = "PAN Number")]
public string? PanNumber { get; set; }

// Employment Details
[Display(Name = "Employment Type")]
public string? EmploymentType { get; set; }

[Display(Name = "Designation")]
public string? Designation { get; set; }

[Display(Name = "Plant Location")]
public string? PlantLocation { get; set; }

[Display(Name = "Joining Date")]
[DataType(DataType.Date)]
public DateTime? JoiningDate { get; set; }

[Display(Name = "Shift Start Time")]
[DataType(DataType.Time)]
public string? ShiftStartTime { get; set; }

[Display(Name = "Shift End Time")]
[DataType(DataType.Time)]
public string? ShiftEndTime { get; set; }

[Display(Name = "Salary Type")]
public string? SalaryType { get; set; }

[Display(Name = "Salary Amount")]
public decimal? SalaryAmount { get; set; }

    }

   public class ResetPasswordViewModel
    {
        public int UserId { get; set; }

        [Required(ErrorMessage = "New password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "Confirm password is required")]
        [DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Password and Confirm Password do not match")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; }
    }

}