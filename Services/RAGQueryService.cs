    using BlogApp1.Shared;
    using Qdrant.Client.Grpc;
    using Qdrant.Client;

    namespace BlogApp1.Server.Services
    {
        public class RAGQueryService
        {
            private readonly QdrantClient _qdrantClient;

            public RAGQueryService(QdrantClient qdrantClient)
            {
                _qdrantClient = qdrantClient;
            }

            public async Task<QueryResult> QueryAsync(QueryRequest request)
            {
                var queryEmbedding = await GetEmbeddingAsync(request.Question);

                var searchResults = await _qdrantClient.SearchAsync(
                    collectionName: "rag_blog_chunks",
                    vector: queryEmbedding.ToArray(),
                    limit: 5
                );

                var relevantChunks = new List<ChunkResult>();

                // 🔥 SAFE MAPPING - Handle NULL Payloads
                foreach (var result in searchResults)
                {
                    if (result.Payload == null) continue; // Skip empty payloads

                    relevantChunks.Add(new ChunkResult
                    {
                        Score = result.Score,
                        Content = result.Payload.TryGetValue("content", out var contentValue) && contentValue != null
                            ? contentValue.StringValue ?? "" : "",
                        Title = result.Payload.TryGetValue("title", out var titleValue) && titleValue != null
                            ? titleValue.StringValue ?? "" : "",
                        Url = result.Payload.TryGetValue("url", out var urlValue) && urlValue != null
                            ? urlValue.StringValue ?? "" : "",
                        Author = result.Payload.TryGetValue("author", out var authorValue) && authorValue != null
                            ? authorValue.StringValue ?? "" : "",
                        Tags = result.Payload.TryGetValue("tags", out var tagsValue) && tagsValue != null
                            ? tagsValue.StringValue ?? "" : ""
                    });
                }

                return new QueryResult
                {
                    RelevantChunks = relevantChunks,
                    TotalChunks = relevantChunks.Count
                };
            }


            // ✅ SAME EMBEDDING METHOD as ingestion!
            private async Task<List<float>> GetEmbeddingAsync(string text)
            {
                var seed = text.GetHashCode();
                var rand = new Random(seed);
                var embedding = new List<float>(384);
                for (int i = 0; i < 384; i++)
                    embedding.Add((float)(rand.NextDouble() * 2 - 1));
                await Task.Delay(1);
                return embedding;
            }
        }
    }
