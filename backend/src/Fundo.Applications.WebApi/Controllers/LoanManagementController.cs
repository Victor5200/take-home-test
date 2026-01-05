using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<LoansController> _logger;

        public LoansController(LoanDbContext context, ILogger<LoansController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all loans
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<LoanDto>), 200)]
        public async Task<ActionResult<IEnumerable<LoanDto>>> GetLoans()
        {
            _logger.LogInformation("Retrieving all loans");

            var loans = await _context.Loans
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {LoanCount} loans", loans.Count);

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
            _logger.LogInformation("Retrieving loan with ID {LoanId}", id);

            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                _logger.LogWarning("Loan with ID {LoanId} not found", id);
                return NotFound(new { message = $"Loan with ID {id} not found" });
            }

            _logger.LogInformation("Successfully retrieved loan {LoanId} for applicant {ApplicantName}", id, loan.ApplicantName);
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
            _logger.LogInformation("Creating new loan for {ApplicantName} with amount {Amount}",
                createLoanDto.ApplicantName, createLoanDto.Amount);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid model state for loan creation: {ApplicantName}", createLoanDto.ApplicantName);
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

            _logger.LogInformation("Successfully created loan {LoanId} for {ApplicantName} with amount {Amount}",
                loan.Id, loan.ApplicantName, loan.Amount);

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
            _logger.LogInformation("Processing payment of {PaymentAmount} for loan {LoanId}", paymentDto.Amount, id);

            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid payment data for loan {LoanId}", id);
                return BadRequest(ModelState);
            }

            var loan = await _context.Loans.FindAsync(id);

            if (loan == null)
            {
                _logger.LogWarning("Payment failed: Loan {LoanId} not found", id);
                return NotFound(new { message = $"Loan with ID {id} not found" });
            }

            if (loan.Status == "paid")
            {
                _logger.LogWarning("Payment rejected: Loan {LoanId} is already paid", id);
                return BadRequest(new { message = "This loan has already been paid in full" });
            }

            if (paymentDto.Amount > loan.CurrentBalance)
            {
                _logger.LogWarning("Payment rejected: Amount {PaymentAmount} exceeds balance {CurrentBalance} for loan {LoanId}",
                    paymentDto.Amount, loan.CurrentBalance, id);
                return BadRequest(new
                {
                    message = "Payment amount exceeds current balance",
                    currentBalance = loan.CurrentBalance,
                    paymentAmount = paymentDto.Amount
                });
            }

            var oldBalance = loan.CurrentBalance;

            // Deduct payment from current balance
            loan.CurrentBalance -= paymentDto.Amount;
            loan.UpdatedAt = DateTime.UtcNow;

            // Update status to 'paid' if balance reaches zero
            if (loan.CurrentBalance == 0)
            {
                loan.Status = "paid";
                _logger.LogInformation("Loan {LoanId} fully paid off. Final payment: {PaymentAmount}", id, paymentDto.Amount);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Payment processed successfully for loan {LoanId}. Amount: {PaymentAmount}, Old Balance: {OldBalance}, New Balance: {NewBalance}",
                id, paymentDto.Amount, oldBalance, loan.CurrentBalance);

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