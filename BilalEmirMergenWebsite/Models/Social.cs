using Postgrest.Attributes;
using Postgrest.Models;

namespace BilalEmirMergenWebsite.Models
{
    [Table("social_links")]
    public class Social : BaseModel
    {
        [PrimaryKey("id", false)]
        public string Id { get; set; } = string.Empty;

        [Column("platform")]
        public string Name { get; set; } = string.Empty;

        [Column("icon")]
        public string Icon { get; set; } = string.Empty; // e.g., "github", "linkedin", "mail", "globe"

        [Column("url")]
        public string Url { get; set; } = string.Empty;
    }
}
