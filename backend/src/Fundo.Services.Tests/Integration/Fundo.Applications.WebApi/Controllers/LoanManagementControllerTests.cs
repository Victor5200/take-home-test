using FluentAssertions;
using Fundo.Applications.WebApi.Data;
using Fundo.Applications.WebApi.DTOs;
using Fundo.Applications.WebApi.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace Fundo.Services.Tests.Integration
{
    public class LoanManagementControllerTests : IClassFixture<WebApplicationFactory<Fundo.Applications.WebApi.Startup>>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Fundo.Applications.WebApi.Startup> _factory;

        public LoanManagementControllerTests(WebApplicationFactory<Fundo.Applications.WebApi.Startup> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the app's DbContext registration
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<LoanDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<LoanDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb");
                    });

                    // Build the service provider and seed test data
                    var sp = services.BuildServiceProvider();
                    using var scope = sp.CreateScope();
                    var scopedServices = scope.ServiceProvider;
                    var db = scopedServices.GetRequiredService<LoanDbContext>();

                    db.Database.EnsureCreated();
                    SeedTestData(db);
                });
            });

            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        private static void SeedTestData(LoanDbContext context)
        {
            context.Loans.RemoveRange(context.Loans);
            context.Loans.AddRange(
                new Loan
                {
                    Id = 1,
                    Amount = 10000.00m,
                    CurrentBalance = 7500.00m,
                    ApplicantName = "John Doe",
                    Status = "active",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                },
                new Loan
                {
                    Id = 2,
                    Amount = 25000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Jane Smith",
                    Status = "paid",
                    CreatedAt = DateTime.UtcNow.AddMonths(-12)
                }
            );
            context.SaveChanges();
        }

        [Fact]
        public async Task GetLoans_ShouldReturnAllLoans()
        {
            // Act
            var response = await _client.GetAsync("/api/loans");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var loans = await response.Content.ReadFromJsonAsync<List<LoanDto>>();
            loans.Should().NotBeNull();
            loans.Should().HaveCountGreaterThanOrEqualTo(2);
        }

        [Fact]
        public async Task GetLoan_WithValidId_ShouldReturnLoan()
        {
            // Act
            var response = await _client.GetAsync("/api/loans/1");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
            loan.Should().NotBeNull();
            loan.Id.Should().Be(1);
            loan.ApplicantName.Should().Be("John Doe");
        }

        [Fact]
        public async Task GetLoan_WithInvalidId_ShouldReturnNotFound()
        {
            // Act
            var response = await _client.GetAsync("/api/loans/999");

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task CreateLoan_WithValidData_ShouldReturnCreated()
        {
            // Arrange
            var newLoan = new CreateLoanDto
            {
                Amount = 30000.00m,
                ApplicantName = "Test User"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans", newLoan);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Created);
            var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
            loan.Should().NotBeNull();
            loan.Amount.Should().Be(30000.00m);
            loan.CurrentBalance.Should().Be(30000.00m);
            loan.ApplicantName.Should().Be("Test User");
            loan.Status.Should().Be("active");
        }

        [Fact]
        public async Task CreateLoan_WithInvalidData_ShouldReturnBadRequest()
        {
            // Arrange
            var invalidLoan = new CreateLoanDto
            {
                Amount = -100.00m, // Invalid amount
                ApplicantName = ""
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans", invalidLoan);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MakePayment_WithValidAmount_ShouldUpdateBalance()
        {
            // Arrange
            var payment = new PaymentDto { Amount = 2500.00m };

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans/1/payment", payment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
            loan.Should().NotBeNull();
            loan.CurrentBalance.Should().Be(5000.00m); // 7500 - 2500
            loan.Status.Should().Be("active");
        }

        [Fact]
        public async Task MakePayment_ThatPaysOffLoan_ShouldUpdateStatusToPaid()
        {
            // Arrange
            var payment = new PaymentDto { Amount = 7500.00m };

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans/1/payment", payment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            var loan = await response.Content.ReadFromJsonAsync<LoanDto>();
            loan.Should().NotBeNull();
            loan.CurrentBalance.Should().Be(0.00m);
            loan.Status.Should().Be("paid");
        }

        [Fact]
        public async Task MakePayment_WithAmountExceedingBalance_ShouldReturnBadRequest()
        {
            // Arrange
            var payment = new PaymentDto { Amount = 10000.00m }; // More than current balance

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans/1/payment", payment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MakePayment_OnPaidLoan_ShouldReturnBadRequest()
        {
            // Arrange
            var payment = new PaymentDto { Amount = 100.00m };

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans/2/payment", payment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task MakePayment_WithInvalidLoanId_ShouldReturnNotFound()
        {
            // Arrange
            var payment = new PaymentDto { Amount = 100.00m };

            // Act
            var response = await _client.PostAsJsonAsync("/api/loans/999/payment", payment);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
