using System;
using System.Collections.Generic;

namespace BlogApp1.Shared
{
    public class BlogDto
    {
        // 🔑 Identity
        public int Id { get; set; }

        // 📝 Core content
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        // 👤 Author
        public string AuthorName { get; set; } = string.Empty;
        public string AuthorUid { get; set; } = string.Empty;

        // 🏷 Categorization
        public List<string> Tags { get; set; } = new();
        public string Domain { get; set; } = "General";

        // 📊 Analytics
        public int ViewCount { get; set; }
        public int LikesCount { get; set; }

        // 🔄 Workflow
        public string? Status { get; set; }   // draft | pending | approved | rejected | revise
        public bool IsPublished { get; set; }

        // ⏱ Reading info
        public long? ReadingTime { get; set; }
        public int? WordCount { get; set; }

        // 🧠 SEO / Meta
        public string? MetaDescription { get; set; }
        public string? Summary { get; set; }
        public string? SourceUrl { get; set; }

        // 🕒 Dates
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }

        // ✍ Editor
        public Guid? EditorUid { get; set; }
    }
    public class TrackReadRequest
    {
        public int BlogId { get; set; }
        public Guid UserId { get; set; } // blog_user.user_id
    }
}
