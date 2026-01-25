    using Supabase.Postgrest.Attributes;
    using Supabase.Postgrest.Models;
    using System;

    namespace BlogApp1.Shared
    {
        [Table("blog_comments")]
        public class BlogComment : BaseModel
        {
            [PrimaryKey("comment_uid", false)]
            public Guid CommentUid { get; set; }

            [Column("blog_id")]
            public int BlogId { get; set; }

            [Column("comment_user_id")]
            public Guid CommentUserId { get; set; }

            [Column("parent_comment_uid")]
            public Guid? ParentCommentUid { get; set; }

            [Column("is_parent")]
            public bool IsParent { get; set; }

            [Column("content")]
            public string Content { get; set; } = string.Empty;

            [Column("likes_count")]
            public int LikesCount { get; set; } = 0;

            [Column("created_at")]
            public DateTime? CreatedAt { get; set; }

            [Column("updated_at")]
            public DateTime? UpdatedAt { get; set; }
            [Column("author_name")]
            public string AuthorName { get; set; }
        }
    }
