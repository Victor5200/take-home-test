using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace Fundo.Applications.WebApi.Data
{
    public class LoanDbContext : DbContext
    {
        public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
        {
        }

        public DbSet<Loan> Loans { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Loan entity
            modelBuilder.Entity<Loan>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Amount).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.CurrentBalance).HasColumnType("decimal(18,2)").IsRequired();
                entity.Property(e => e.ApplicantName).HasMaxLength(200).IsRequired();
                entity.Property(e => e.Status).HasMaxLength(20).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Seed data
            modelBuilder.Entity<Loan>().HasData(
                new Loan
                {
                    Id = 1,
                    Amount = 10000.00m,
                    CurrentBalance = 7500.00m,
                    ApplicantName = "John Doe",
                    Status = "active",
                    CreatedAt = System.DateTime.UtcNow.AddMonths(-6)
                },
                new Loan
                {
                    Id = 2,
                    Amount = 25000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Jane Smith",
                    Status = "paid",
                    CreatedAt = System.DateTime.UtcNow.AddMonths(-12)
                },
                new Loan
                {
                    Id = 3,
                    Amount = 50000.00m,
                    CurrentBalance = 35000.00m,
                    ApplicantName = "Robert Johnson",
                    Status = "active",
                    CreatedAt = System.DateTime.UtcNow.AddMonths(-3)
                },
                new Loan
                {
                    Id = 4,
                    Amount = 15000.00m,
                    CurrentBalance = 5000.00m,
                    ApplicantName = "Maria Silva",
                    Status = "active",
                    CreatedAt = System.DateTime.UtcNow.AddMonths(-9)
                },
                new Loan
                {
                    Id = 5,
                    Amount = 75000.00m,
                    CurrentBalance = 0.00m,
                    ApplicantName = "Michael Brown",
                    Status = "paid",
                    CreatedAt = System.DateTime.UtcNow.AddMonths(-18)
                }
            );
        }
    }
}
