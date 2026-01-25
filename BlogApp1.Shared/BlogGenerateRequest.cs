using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
namespace BlogApp1.Shared
{
    public class BlogGenerateRequest
    {
        public string Topic { get; set; } = "";
        public string Tone { get; set; } = "";
        public int WordCount { get; set; }

        // Optional – user can override default behavior
        public string? CustomPrompt { get; set; }
    }

    public class BlogGenerateResponse
    {
        public string Title { get; set; } = "";
        public string Slug { get; set; } = "";
        public string Content { get; set; } = ""; // HTML
        public List<string> Tags { get; set; } = new();
        public string Domain { get; set; } = "";
        public string Meta_Description { get; set; } = "";
        public string Summary { get; set; } = "";
        public string ImageUrl { get; set; } = "";
    }


}
