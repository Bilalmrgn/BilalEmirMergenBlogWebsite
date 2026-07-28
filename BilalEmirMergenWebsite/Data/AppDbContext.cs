using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using BilalEmirMergenWebsite.Models;
using Microsoft.EntityFrameworkCore;

namespace BilalEmirMergenWebsite.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Article> Articles { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<Social> Socials { get; set; } = null!;
        public DbSet<Analytics> Analytics { get; set; } = null!;
        public DbSet<AdminUser> AdminUsers { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Project.Tags list conversion to comma-separated string for SQL Server
            modelBuilder.Entity<Project>()
                .Property(p => p.Tags)
                .HasConversion(
                    v => v == null ? string.Empty : string.Join(',', v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                );

            // Seed default admin user: admin@bilal.com / Bilal.123
            string defaultPasswordHash = HashPassword("Bilal.123");
            modelBuilder.Entity<AdminUser>().HasData(new AdminUser
            {
                Id = "1a8f906f-683a-4467-b5b6-7f414436573c",
                Email = "admin@bilal.com",
                PasswordHash = defaultPasswordHash
            });
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
