using System.ComponentModel.DataAnnotations;

namespace ERPDemo.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string RoleName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}