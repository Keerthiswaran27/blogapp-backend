using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp1.Shared.RAG
{
    public class BlogIngestDto
    {
        public int Id { get; set; }                // blog id (required)
        public string Title { get; set; } = "";    // required
        public string Slug { get; set; } = "";     // required
        public string Content { get; set; } = "";  // required - HTML or plain text
        public string AuthorName { get; set; } = "";// recommended
        public string? AuthorUid { get; set; }     // optional but recommended for stable filters
        public List<string>? Tags { get; set; }    // send as array ["tag1","tag2"]
        public string? Domain { get; set; } = "General";
        public DateTimeOffset? PublishedAt { get; set; }
        public string? SourceUrl { get; set; }
        public string? Summary { get; set; }       // optional, will be prepended to first chunk if present
        public string? MetaDescription { get; set; } // optional — include only if useful
    }
    public class ChunkDto
    {
        public string PointId { get; set; } = string.Empty; // blog_{id}__chunk_{index}
        public int Index { get; set; }
        public int StartChar { get; set; }
        public int EndChar { get; set; }
        public string Text { get; set; } = string.Empty;     // full chunk text
    }

    // Result returned by ingestion endpoint
    public class IngestResultDto
    {
        public bool Ok { get; set; }
        public int ChunksCreated { get; set; }
        public int VectorsUpserted { get; set; }
        public string Collection { get; set; } = string.Empty;
        public object? QdrantResponse { get; set; }
    }

}
