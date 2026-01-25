using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace BlogApp1.Shared
{
    [Table("editor_feedback")]
    public class EditorFeedback : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("blog_id")]
        public int BlogId { get; set; }

        [Column("author_uid")]
        public string AuthorUid { get; set; }

        [Column("editor_uid")]
        public string EditorUid { get; set; }

        [Column("feedback_type")]
        public string FeedbackType { get; set; } = "general";
        // e.g., grammar, formatting, clarity, missing_reference

        [Column("message")]
        public string Message { get; set; }
        [Column("editorname")]
        public string EditorName { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_author_visible")]
        public bool IsAuthorVisible { get; set; } = true;
    }}
