using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class BlogListDto
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string AuthorName { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class DashboardStatsDto
    {
        public int Pending { get; set; }
        public int Approved { get; set; }
        public int Rejected { get; set; }
        public int Revision { get; set; }
    }

    public class EditorActionDto
    {
        public string Message { get; set; } = string.Empty;
    }

    public class EditorCommentDto
    {
        public int BlogId { get; set; }
        public string Message { get; set; }
        public string FeedbackType { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
