using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ERPDemo.Models
{
    public class RefreshToken
    {
        [Key]
        public int TokenId { get; set; }

        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }

        [Required]
        [StringLength(500)]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiresAt { get; set; }

       public DateTime CreatedAt { get; set; }

        public DateTime? RevokedAt { get; set; }

        public bool IsRevoked { get; set; } = false;
    }
}