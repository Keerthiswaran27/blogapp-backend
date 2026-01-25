    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    namespace BlogApp1.Shared
    {
        public class CommentDto
        {
            public Guid CommentUid { get; set; }
            public int BlogId { get; set; }
            public Guid CommentUserId { get; set; }
            public Guid? ParentCommentUid { get; set; }
            public bool IsParent { get; set; }
            public string Content { get; set; } = "";
            public int LikesCount { get; set; }
            public DateTime? CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
            public string? AuthorName { get; set; }
            public string? BlogTitle { get; set; }
    }
    }
