using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fundo.Applications.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class LoansController : ControllerBase
    {
        private readonly LoanDbContext _context;

        public LoansController(LoanDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get all loans
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LoanDto>), 200)]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
        {
            var loans = await _context.Loans
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var loanDtos = loans.Select(l => MapToDto(l)).ToList();
            return Ok(loanDtos);
        }

        /// <summary>
        /// Get a specific loan by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LoanDto>> GetLoan(int id)
        {
            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found" });
            }

            return Ok(MapToDto(loan));
        }

        /// <summary>
        /// Create a new loan
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(LoanDto), 201)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<LoanDto>> CreateLoan([FromBody] CreateLoanDto createLoanDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loan = new Loan
            {
                Amount = createLoanDto.Amount,
                CurrentBalance = createLoanDto.Amount, // Initial balance equals the loan amount
                ApplicantName = createLoanDto.ApplicantName,
                Status = "active",
                CreatedAt = DateTime.UtcNow
            };

            _context.Loans.Add(loan);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetLoan), new { id = loan.Id }, MapToDto(loan));
        }

        /// <summary>
        /// Make a payment towards a loan
        /// </summary>
        [HttpPost("{id}/payment")]
        [ProducesResponseType(typeof(LoanDto), 200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<LoanDto>> MakePayment(int id, [FromBody] PaymentDto paymentDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                return NotFound(new { message = $"Loan with ID {id} not found" });
            }

            if (loan.Status == "paid")
            {
                return BadRequest(new { message = "This loan has already been paid in full" });
            }

            if (paymentDto.Amount > loan.CurrentBalance)
            {
                return BadRequest(new
                {
                    message = "Payment amount exceeds current balance",
                    currentBalance = loan.CurrentBalance,
                    paymentAmount = paymentDto.Amount
                });
            }

            // Deduct payment from current balance
            loan.CurrentBalance -= paymentDto.Amount;
            loan.UpdatedAt = DateTime.UtcNow;

            // Update status to 'paid' if balance reaches zero
            if (loan.CurrentBalance == 0)
            {
                loan.Status = "paid";
            }

            await _context.SaveChangesAsync();

            return Ok(MapToDto(loan));
        }

        private LoanDto MapToDto(Loan loan)
        {
            return new LoanDto
            {
                Id = loan.Id,
                Amount = loan.Amount,
                CurrentBalance = loan.CurrentBalance,
                ApplicantName = loan.ApplicantName,
                Status = loan.Status,
                CreatedAt = loan.CreatedAt,
                UpdatedAt = loan.UpdatedAt
            };
        }
    }
}