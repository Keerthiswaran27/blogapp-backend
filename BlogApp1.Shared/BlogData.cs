using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;

namespace BlogApp1.Shared
{
    [Table("blog_data")]
    public class BlogData : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        // Basic blog fields
        [Column("title")]
        public string Title { get; set; }

        [Column("slug")]
        public string Slug { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        // Author
        [Column("author_name")]
        public string AuthorName { get; set; }

        [Column("author_uid")]
        public string AuthorUid { get; set; }

        // Categorization
        [Column("tags")]
        public List<string> Tags { get; set; } = new();

        [Column("domain")]
        public string Domain { get; set; } = "General";

        // Analytics (kept in table)
        [Column("view_count")]
        public int ViewCount { get; set; } = 0;

        [Column("likes_count")]
        public int   LikesCount { get; set; } = 0;

        // Timestamps
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        // Workflow & meta
        [Column("status")]
        public string? Status { get; set; }   // draft|pending|approved|rejected|revise

        [Column("reading_time")]
        public long? ReadingTime { get; set; }    // kept as bigint in DB

        [Column("meta_description")]
        public string? MetaDescription { get; set; }

        [Column("published_at")]
        public DateTime? PublishedAt { get; set; }

        [Column("editor_uid")]
        public string?  EditorUid { get; set; }

        [Column("reviewed_at")]
        public DateTime? ReviewedAt { get; set; }

        // RAG-related additions
        [Column("summary")]
        public string? Summary { get; set; }

        [Column("source_url")]
        public string? SourceUrl { get; set; }

        [Column("is_published")]
        public bool IsPublished { get; set; } = false;

        [Column("word_count")]
        public int? WordCount { get; set; }
    }
}
