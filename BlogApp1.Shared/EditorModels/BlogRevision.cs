using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;

namespace BlogApp1.Shared
{
    [Table("blog_revisions")]
    public class BlogRevisions : BaseModel
    {
        [PrimaryKey("id", false)]
        public int Id { get; set; }

        [Column("blog_id")]
        public int BlogId { get; set; }

        [Column("version_no")]
        public int VersionNo { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("editor_uid")]
        public string? EditorUid { get; set; }

        [Column("author_uid")]
        public string? AuthorUid { get; set; }

        [Column("is_current")]
        public bool IsCurrent { get; set; } = false;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
