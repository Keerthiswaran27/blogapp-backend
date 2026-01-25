using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared
{
    // Models/IngestionResult.cs
    public class IngestionResult
    {
        public bool Success { get; set; }
        public int PointsCreated { get; set; }
        public string[] CustomIds { get; set; } = Array.Empty<string>();
    }
    // ✅ ADD THIS CLASS (same file as RAGIngestionService)
    public class Chunk
    {
        public string Content { get; set; } = "";
        public int ChunkIndex { get; set; }
        public int TotalChunks { get; set; }
    }

}
