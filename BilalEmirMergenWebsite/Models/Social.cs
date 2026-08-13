using System;
using System.ComponentModel.DataAnnotations;

namespace BilalEmirMergenWebsite.Models
{
    public class Social : IOrderedContent
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        [Required, StringLength(80)]
        public string Name { get; set; } = string.Empty;
        [Required, StringLength(80)]
        public string Icon { get; set; } = "link";
        [Required, StringLength(500)]
        public string Url { get; set; } = string.Empty;
        public int SortOrder { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
