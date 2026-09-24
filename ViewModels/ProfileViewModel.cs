using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class ProfileViewModel
    {
        public int UserId { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Username")]
        public string Username { get; set; } = string.Empty;

        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(150)]
        [Display(Name = "Full Name")]
        public string? FullName { get; set; }

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        // Read-only display info
        public string RoleName { get; set; } = string.Empty;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}