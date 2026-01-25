using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    public class NewBlog
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public string Image { get; set; }
        public string AuhtorName { get; set;}
        public string AuhtorUID { get; set;}
        public List<string> Tags { get; set; } = new();
        public string Domain { get; set; }
        public string Status { get; set; }
        public string MetaDescription { get; set; }

    }
    public class NewBlog1
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public string AuthorName { get; set; } = string.Empty;

        public string AuthorUid { get; set; } = string.Empty;

        public List<string> Tags { get; set; } = new();

        public string Domain { get; set; } = "General";

        public string? MetaDescription { get; set; }

        public string? Summary { get; set; }

        public long? ReadingTime { get; set; }

        public int? WordCount { get; set; }
    }
    public class NewBlogRequest
    {
        public string Title { get; set; } = string.Empty;

        public string Slug { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string? CoverImageUrl { get; set; }

        public List<string> Tags { get; set; } = new();

        public string Domain { get; set; } = "General";

        public string? MetaDescription { get; set; }

        public string? Summary { get; set; }
    }
}
