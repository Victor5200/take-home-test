using System;
using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        [StringLength(50)]
        public string Role { get; set; } = "User"; // User or Admin

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
