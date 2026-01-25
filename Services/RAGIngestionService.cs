using BlogApp1.Shared;
using Qdrant.Client.Grpc;
using System.Text;
using Qdrant.Client;
using BlogApp1.Shared;
using System.Text.RegularExpressions;

namespace BlogApp1.Server.Services
{
    // Services/RAGIngestionService.cs
    public class RAGIngestionService
    {
        private readonly QdrantClient _qdrantClient;

        public RAGIngestionService(QdrantClient qdrantClient)
        {
            _qdrantClient = qdrantClient;
        }

        public async Task<IngestionResult> IngestBlogAsync(BlogIngestionRequest request)
        {
            string cleanContent = ProductionClean(request.Content);
            var chunks = ChunkContent(cleanContent, 700, 100);

            var allPoints = new List<PointStruct>();
            var customIds = new List<string>();

            foreach (var chunk in chunks)
            {
                var embedding = await GetEmbeddingAsync(chunk.Content);

                var point = new PointStruct
                {
                    Id = new PointId { Uuid = Guid.NewGuid().ToString() },
                    Vectors = new Vectors
                    {
                        Vector = new Vector { Data = { embedding } }
                    }
                };

                // ✅ ALL PAYLOAD FIELDS + URL!
                point.Payload.Add("title", new Value { StringValue = request.Title });
                point.Payload.Add("author", new Value { StringValue = request.Author });
                point.Payload.Add("domain", new Value { StringValue = request.Domain });
                point.Payload.Add("url", new Value { StringValue = request.Url });           // ✅ URL!
                point.Payload.Add("content", new Value { StringValue = chunk.Content });
                point.Payload.Add("tags", new Value { StringValue = string.Join(",", request.Tags) });
                point.Payload.Add("post_id", new Value { IntegerValue = request.Title.GetHashCode() });
                point.Payload.Add("chunk_index", new Value { IntegerValue = chunk.ChunkIndex });
                point.Payload.Add("total_chunks", new Value { IntegerValue = chunk.TotalChunks });
                point.Payload.Add("custom_id", new Value { StringValue = $"post_{request.Title.GetHashCode()}_chunk_{chunk.ChunkIndex}" });

                allPoints.Add(point);
                customIds.Add($"post_{request.Title.GetHashCode()}_chunk_{chunk.ChunkIndex}");
            }

            await _qdrantClient.UpsertAsync("blog_chunks", allPoints);

            // ✅ CREATE URL INDEX TOO!
            await _qdrantClient.CreatePayloadIndexAsync("blog_chunks", "url", PayloadSchemaType.Keyword);

            return new IngestionResult
            {
                Success = true,
                PointsCreated = allPoints.Count,
                CustomIds = customIds.ToArray()
            };
        }
        // Cleans the HTML content by removing tags, scripts, styles, and decoding entities
        string ProductionClean(string html)
        {
            html = Regex.Replace(html, @"<script[^>]*>.*?</script>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<style[^>]*>.*?</style>", "", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            html = Regex.Replace(html, @"<!--.*?-->", "", RegexOptions.Singleline);
            html = Regex.Replace(html, @"<[^>]*>", "");
            html = html.Replace("&quot;", "\"")
                       .Replace("&ldquo;", "\"")
                       .Replace("&rdquo;", "\"")
                       .Replace("&apos;", "'")
                       .Replace("&lsquo;", "'")
                       .Replace("&rsquo;", "'")
                       .Replace("&amp;", "&")
                       .Replace("&lt;", "<")
                       .Replace("&gt;", ">")
                       .Replace("&nbsp;", " ")
                       .Replace(" ", " "); // Non-breaking space to normal space
            return Regex.Replace(html.Replace("\u00A0", " "), @"\s+", " ").Trim();
        }

        // Splits a large content string into chunks with a maximum size and some overlap between chunks
        List<Chunk> ChunkContent(string content, int maxChunkSize = 700, int overlap = 100)
        {
            var separators = new[] { "\n\n", "\n", ". ", " " };
            var textChunks = RecursiveSplit(content, separators, maxChunkSize, overlap);
            var chunks = new List<Chunk>();
            for (int i = 0; i < textChunks.Count; i++)
            {
                chunks.Add(new Chunk
                {
                    Content = textChunks[i],
                    ChunkIndex = i + 1,
                    TotalChunks = textChunks.Count
                });
            }
            return chunks;
        }

        // Recursive method to split text using an array of separators until chunks are small enough
        List<string> RecursiveSplit(string text, string[] separators, int maxSize, int overlap)
        {
            if (text.Length <= maxSize)
                return new List<string> { text };

            foreach (var separator in separators)
            {
                var parts = text.Split(new[] { separator }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 1) continue;

                var chunks = new List<string>();
                var currentChunk = new StringBuilder();
                foreach (var part in parts)
                {
                    string testChunk = (currentChunk.Length == 0) ? part : currentChunk.ToString() + separator + part;
                    if (testChunk.Length > maxSize)
                    {
                        if (currentChunk.Length > 0)
                        {
                            chunks.Add(currentChunk.ToString().Trim());
                            var overlapText = currentChunk.Length > overlap
                                ? currentChunk.ToString(currentChunk.Length - overlap, overlap)
                                : currentChunk.ToString();
                            currentChunk.Clear();
                            currentChunk.Append(overlapText);
                        }
                        currentChunk.Append(part);
                    }
                    else
                    {
                        if (currentChunk.Length > 0)
                            currentChunk.Append(separator);
                        currentChunk.Append(part);
                    }
                }
                if (currentChunk.Length > 0)
                    chunks.Add(currentChunk.ToString().Trim());

                if (chunks.Any(c => c.Length > maxSize) && separators.Length > 1)
                    return RecursiveSplit(string.Join(separator, chunks), separators.Skip(1).ToArray(), maxSize, overlap);
                return chunks;
            }

            // Fallback: simple split by size if none of the separators work well
            var fallback = new List<string>();
            int index = 0;
            while (index < text.Length)
            {
                int length = Math.Min(maxSize, text.Length - index);
                fallback.Add(text.Substring(index, length));
                index += (maxSize - overlap);
            }
            return fallback;
        }

        // Simulated method to generate a 384-dimensional vector embedding from text for testing/demo
        async Task<List<float>> GetEmbeddingAsync(string text)
        {
            var seed = text.GetHashCode();
            var rand = new Random(seed);
            var embedding = new List<float>(384);
            for (int i = 0; i < 384; i++)
            {
                embedding.Add((float)(rand.NextDouble() * 2 - 1));
            }
            await Task.Delay(1);
            return embedding;
        }


    }


}
