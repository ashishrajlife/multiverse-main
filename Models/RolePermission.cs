using System.ComponentModel.DataAnnotations;

namespace ERPDemo.Models
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }
        
        public int RoleId { get; set; }
        public Role? Role { get; set; }

        [Required, StringLength(50)]
        public string ModuleName { get; set; } = string.Empty;
    }
}