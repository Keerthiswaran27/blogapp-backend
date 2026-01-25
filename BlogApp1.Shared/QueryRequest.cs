using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    // Models/QueryRequest.cs
    public class QueryRequest
    {
        public string Question { get; set; } = "";
    }
    // Models/QueryResult.cs
    public class QueryResult
    {
        public List<ChunkResult> RelevantChunks { get; set; } = new();
        public int TotalChunks { get; set; }
    }
    // Models/ChunkResult.cs
    public class ChunkResult
    {
        public float Score { get; set; }
        public string Content { get; set; } = "";
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public string Author { get; set; } = "";
        public string Tags { get; set; } = "";
    }

}
