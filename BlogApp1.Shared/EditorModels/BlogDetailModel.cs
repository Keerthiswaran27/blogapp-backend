using System;
using System.Collections.Generic;

namespace BlogApp1.Shared.EditorModels
{
    public class BlogDetailModel
    {
        public int Id { get; set; }

        // 📝 Basic Info
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUid { get; set; } = string.Empty;
        public string Domain { get; set; } = "General";
        public List<string> Tags { get; set; } 
        public string? CoverImageUrl { get; set; }

        // 🧩 SEO + Meta Fields
        public string Slug { get; set; } = string.Empty;
        public string MetaDescription { get; set; } = string.Empty;

        // ⚙️ Review + Status Info
        public string Status { get; set; } = "pending";
        public string? ReviewComments { get; set; }
        public string? RejectionReason { get; set; }
        public string? EditorUid { get; set; }

        // 📊 Metrics
        public long? ReadingTime { get; set; }

        // 🕒 Timestamps
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
    }
}
