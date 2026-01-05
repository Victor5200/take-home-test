using System.ComponentModel.DataAnnotations;

namespace Fundo.Applications.WebApi.DTOs
{
    public class CreateLoanDto
    {
        [Required(ErrorMessage = "Amount is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required(ErrorMessage = "Applicant name is required")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Applicant name must be between 2 and 200 characters")]
        public string ApplicantName { get; set; }
    }
}
