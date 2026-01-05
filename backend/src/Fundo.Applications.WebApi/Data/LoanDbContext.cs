using Fundo.Applications.WebApi.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;

namespace Fundo.Applications.WebApi.Data
{
    public class LoanDbContext : DbContext
    {
        public LoanDbContext(DbContextOptions<LoanDbContext> options) : base(options)
        {
        }

        public DbSet<Loan> Loans { get; set; }
        public DbSet<User> Users { get; set; }

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

            // Configure User entity
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Username).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Username).HasMaxLength(100).IsRequired();
                entity.Property(e => e.Email).HasMaxLength(200).IsRequired();
                entity.Property(e => e.PasswordHash).IsRequired();
                entity.Property(e => e.Role).HasMaxLength(50).IsRequired();
                entity.Property(e => e.CreatedAt).IsRequired();
            });

            // Seed Loans
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

            // Seed Users (admin and regular user for testing)
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    Username = "admin",
                    Email = "admin@loanmanagement.com",
                    PasswordHash = HashPassword("admin123"),
                    Role = "Admin",
                    CreatedAt = DateTime.UtcNow.AddMonths(-12)
                },
                new User
                {
                    Id = 2,
                    Username = "testuser",
                    Email = "user@loanmanagement.com",
                    PasswordHash = HashPassword("user123"),
                    Role = "User",
                    CreatedAt = DateTime.UtcNow.AddMonths(-6)
                }
            );
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
