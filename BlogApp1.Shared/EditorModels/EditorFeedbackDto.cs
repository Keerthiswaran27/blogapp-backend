using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared.EditorModels
{
    public class EditorFeedbackDto
    {
        public int Id { get; set; }
        public int BlogId { get; set; }
        public string AuthorUid { get; set; } = string.Empty;
        public string EditorUid { get; set; } = string.Empty;
        public string FeedbackType { get; set; } = "general";
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsAuthorVisible { get; set; } = true;
        public string EditorName { get; set; }

    }
    public class ReviseBlogRequest
    {
        public int BlogId { get; set; }
        public string EditorUid { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string EditorName { get; set; } = string.Empty;
    }
    public class ApproveBlogRequest
    {
        public int BlogId { get; set; }
        public string EditorUid { get; set; } = string.Empty;
    }
    public class RejectBlogRequest
    {
        public int BlogId { get; set; }
        public string EditorUid { get; set; } = string.Empty;
        public string Comment { get; set; } = string.Empty;
        public string EditorName { get; set; } = string.Empty;
    }

}
