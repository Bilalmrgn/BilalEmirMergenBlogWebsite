using System.ComponentModel.DataAnnotations;

namespace BilalEmirMergenWebsite.Models
{
    public class Article : IOrderedContent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required] public string Title { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string TitleTr { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string CategoryEn { get; set; } = string.Empty;
        public string CategoryTr { get; set; } = string.Empty;
        public string CategoryAr { get; set; } = string.Empty;
        [Required] public string Slug { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string SummaryEn { get; set; } = string.Empty;
        public string SummaryTr { get; set; } = string.Empty;
        public string SummaryAr { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string ContentEn { get; set; } = string.Empty;
        public string ContentTr { get; set; } = string.Empty;
        public string ContentAr { get; set; } = string.Empty;
        public string CoverImage { get; set; } = string.Empty;
        public string ExternalUrl { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string ContentLanguage { get; set; } = "tr";
        public bool IsFeatured { get; set; }
        public bool IsInternal { get; set; } = true;
        public bool IsActive { get; set; } = true;
        public int SortOrder { get; set; }
        public int Views { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? PublishedAt { get; set; }
        public string SeoTitle { get; set; } = string.Empty;
        public string SeoDescription { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
