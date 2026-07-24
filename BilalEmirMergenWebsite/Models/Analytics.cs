using Postgrest.Attributes;
using Postgrest.Models;

namespace BilalEmirMergenWebsite.Models
{
    [Table("analytics")]
    public class Analytics : BaseModel
    {
        [PrimaryKey("id", true)]
        public string Id { get; set; } = string.Empty;

        [Column("event_type")]
        public string EventType { get; set; } = "page_view";

        [Column("page_path")]
        public string PagePath { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
