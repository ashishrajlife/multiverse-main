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

        [Required(ErrorMessage = "Password required hai"), StringLength(255, MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [StringLength(150)]
        public string? FullName { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Role select karo")]
        public int RoleId { get; set; }
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
    }

    public class ResetPasswordViewModel
    {
        [Required]
        public int UserId { get; set; }

        [Required, StringLength(255, MinimumLength = 4)]
        [DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;
    }
}