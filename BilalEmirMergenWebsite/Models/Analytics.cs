using System;

namespace BilalEmirMergenWebsite.Models
{
    public class Analytics
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string EventType { get; set; } = "page_view";
        public string PagePath { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
