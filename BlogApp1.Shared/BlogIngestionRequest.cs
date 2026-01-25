using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    // Models/BlogIngestionRequest.cs
    public class BlogIngestionRequest
    {
        public string Title { get; set; } = "";
        public string Author { get; set; } = "";
        public string Domain { get; set; } = "";
        public string Url { get; set; } = "";           // ✅ NEW: Blog URL!
        public string Content { get; set; } = "";       // HTML/raw text
        public string[] Tags { get; set; } = Array.Empty<string>();
    }

}
