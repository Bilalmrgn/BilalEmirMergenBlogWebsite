using Postgrest.Attributes;
using Postgrest.Models;

namespace BilalEmirMergenWebsite.Models
{
    [Table("projects")]
    public class Project : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; } = string.Empty;

        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("project_url")]
        public string ProjectUrl { get; set; } = string.Empty;

        [Column("tags")]
        public List<string> Tags { get; set; } = new List<string>();

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
