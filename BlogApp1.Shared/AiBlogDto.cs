using System;
using System.Collections.Generic;

namespace BlogApp1.Shared
{
    public class AIBlogDto
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string MetaDescription { get; set; }
        public string Category { get; set; }
        public List<string> Tags { get; set; } = new();
        public string AuthorName { get; set; }
        public string AuthorUid { get; set; }
        public string CoverImageUrl { get; set; }
        public string ReadingTime { get; set; }  // keep string for now like "4 minutes"
    }
}
