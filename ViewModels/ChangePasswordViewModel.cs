using System.ComponentModel.DataAnnotations;

namespace ERPDemo.ViewModels
{
    public class ChangePasswordViewModel
    {
        [Required, DataType(DataType.Password)]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required, StringLength(255, MinimumLength = 4), DataType(DataType.Password)]
        public string NewPassword { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage = "Passwords match nahi kar rahe.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}