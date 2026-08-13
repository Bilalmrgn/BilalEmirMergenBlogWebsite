using System.ComponentModel.DataAnnotations;

namespace BilalEmirMergenWebsite.Models
{
    public class Project : IOrderedContent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required] public string Title { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleTr { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string CategoryEn { get; set; } = string.Empty;
        public string CategoryTr { get; set; } = string.Empty;
        public string CategoryAr { get; set; } = string.Empty;
        [Required, MaxLength(180)] public string Slug { get; set; } = string.Empty;
        public string ShortDescriptionEn { get; set; } = string.Empty;
        public string ShortDescriptionTr { get; set; } = string.Empty;
        public string ShortDescriptionAr { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DescriptionEn { get; set; } = string.Empty;
        public string DescriptionTr { get; set; } = string.Empty;
        public string DescriptionAr { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string Screenshots { get; set; } = string.Empty;
        public string ProjectUrl { get; set; } = string.Empty;
        public string GitHubUrl { get; set; } = string.Empty;
        public string CaseStudyUrl { get; set; } = string.Empty;
        public string Architecture { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsActive { get; set; } = true;
        public string Status { get; set; } = "Completed";
        public int SortOrder { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
