using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class NewComment
    {
        public Guid? CommentUid { get; set; }
        public int BlogId { get; set; }
        public string CommentUserId { get; set; }
        public string ParentCommentUid { get; set; }
        public bool IsParent { get; set; }
        public string Content { get; set; } = "";
        public int LikesCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
