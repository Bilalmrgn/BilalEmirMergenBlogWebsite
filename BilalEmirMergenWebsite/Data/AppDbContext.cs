using System;
using System.Collections.Generic;
using System.Linq;
using BilalEmirMergenWebsite.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

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
        public DbSet<Experience> Experiences { get; set; } = null!;
        public DbSet<Education> Educations { get; set; } = null!;
        public DbSet<SkillCategory> SkillCategories { get; set; } = null!;
        public DbSet<Skill> Skills { get; set; } = null!;
        public DbSet<EngineeringConcept> EngineeringConcepts { get; set; } = null!;
        public DbSet<SpokenLanguage> SpokenLanguages { get; set; } = null!;
        public DbSet<SiteSettings> SiteSettings { get; set; } = null!;
        public DbSet<AboutSection> AboutSections { get; set; } = null!;
        public DbSet<AcademicFoundation> AcademicFoundations { get; set; } = null!;
        public DbSet<Service> Services { get; set; } = null!;
        public DbSet<Certificate> Certificates { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // Always target the same schema regardless of the SQL login's default schema.
            // Without this, production created/read bilalmrgn.* while local development
            // created/read dbo.*, splitting portfolio content across duplicate tables.
            modelBuilder.HasDefaultSchema("dbo");

            // Configure Project.Tags list conversion to comma-separated string for SQL Server
            modelBuilder.Entity<Project>()
                .Property(p => p.Tags)
                .HasConversion(
                    v => v == null ? string.Empty : string.Join(',', v),
                    v => string.IsNullOrEmpty(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (left, right) => left != null && right != null && left.SequenceEqual(right),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    value => value.ToList()));

            modelBuilder.Entity<Project>().HasIndex(p => p.Slug);
            modelBuilder.Entity<Article>().HasIndex(a => a.Slug);
            modelBuilder.Entity<Article>().HasIndex(a => new { a.IsActive, a.PublishedAt });
            modelBuilder.Entity<Certificate>().HasIndex(c => new { c.IsActive, c.SortOrder });
            modelBuilder.Entity<Project>().HasIndex(p => new { p.IsActive, p.SortOrder });

            modelBuilder.Entity<Skill>().Property(skill => skill.YearsOfExperience).HasPrecision(4, 1);
            modelBuilder.Entity<Skill>()
                .HasOne(s => s.Category)
                .WithMany(c => c.Skills)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
