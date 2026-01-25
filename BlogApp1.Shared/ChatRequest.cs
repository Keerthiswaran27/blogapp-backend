using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    // Models/ChatRequest.cs
    public class ChatRequest
    {
        public string Question { get; set; } = "";
    }

    // Models/ChatResponse.cs
    public class ChatResponse
    {
        public string Answer { get; set; } = "";
        public List<Source> Sources { get; set; } = new();
        public string SourcesSummary { get; set; } = "";  // "Used 3 sources"
    }

    public class Source
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public float Score { get; set; }
        public int SourceNumber { get; set; }
    }
    public class OllamaBlogRequest
    {
        public string Topic { get; set; } = string.Empty;
        public int WordCount { get; set; } = 600;
    }

    // For non-stream path, keep this for later reuse
    public class OllamaBlogResponse
    {
        public string Content { get; set; } = string.Empty;
    }

}
