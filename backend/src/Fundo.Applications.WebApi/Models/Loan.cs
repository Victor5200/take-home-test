using System;
using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.Models
{
    public class Loan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Current balance cannot be negative")]
        public decimal CurrentBalance { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string ApplicantName { get; set; }

        [Required]
        [RegularExpression("^(active|paid)$", ErrorMessage = "Status must be 'active' or 'paid'")]
        public string Status { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
