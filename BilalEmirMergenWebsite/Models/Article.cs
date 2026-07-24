using Postgrest.Attributes;
using Postgrest.Models;

namespace BilalEmirMergenWebsite.Models
{
    [Table("articles")]
    public class Article : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("slug")]
        public string Slug { get; set; } = string.Empty;

        [Column("summary")]
        public string Summary { get; set; } = string.Empty;

        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [Column("cover_image")]
        public string CoverImage { get; set; } = string.Empty;

        [Column("views")]
        public int Views { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
