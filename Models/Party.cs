using System.ComponentModel.DataAnnotations;

namespace ERPDemo.Models
{
    public class Party
    {
        [Key]
        public int PartyId { get; set; }

        [Required, StringLength(150)]
        [Display(Name = "Party Name")]
        public string PartyName { get; set; } = string.Empty;

        [StringLength(20)]
        [Display(Name = "Phone Number")]
        public string? PhoneNumber { get; set; }

        [StringLength(50)]
        [Display(Name = "GSTIN")]
        public string? Gstin { get; set; }

        [StringLength(250)]
        [Display(Name = "Address")]
        public string? Address { get; set; }

        [StringLength(100)]
        [Display(Name = "City")]
        public string? City { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}