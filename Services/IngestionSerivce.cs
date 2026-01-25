using BlogApp1.Shared;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

public class IngestionService
{
    private readonly QdrantClient _qdrant;

    private const string COLLECTION = "rag_blog_chunks";

    public IngestionService(QdrantClient qdrant)
    {
        _qdrant = qdrant;
    }

    // 1️⃣ Parse HTML
    public string ParseHtml(string html)
    {
        return Regex.Replace(html, "<.*?>", "");
    }

    // 2️⃣ Chunk WITH overlap
    public List<string> ChunkText(string text)
    {
        int chunkSize = 200;
        int overlap = 20;

        var words = text.Split(" ", StringSplitOptions.RemoveEmptyEntries);
        var chunks = new List<string>();

        int start = 0;

        while (start < words.Length)
        {
            var chunkWords = words
                .Skip(start)
                .Take(chunkSize)
                .ToArray();

            chunks.Add(string.Join(" ", chunkWords));

            start += (chunkSize - overlap);
        }

        return chunks;
    }


    // 3️⃣ Generate embedding (keep your Python API)
    public async Task<float[]> GenerateEmbedding(string text)
    {
        using var http = new HttpClient();

        var payload = new { text };
        var json = JsonSerializer.Serialize(payload);

        var content = new StringContent(
            json,
            Encoding.UTF8,
            "application/json"
        );

        var response = await http.PostAsync(
            "http://localhost:8000/embed",
            content);

        var result = await response.Content.ReadAsStringAsync();
        var obj = JsonSerializer.Deserialize<EmbeddingResponse>(result);

        return obj.embedding;
    }

    // 4️⃣ INSERT USING QDRANT SDK ⭐
    public async Task InsertIntoQdrant(
        float[] vector,
        string chunk,
        RagIngestionDto dto,
        int index,
        int total)
    {
        var point = new PointStruct
        {
            Id = new PointId { Uuid = Guid.NewGuid().ToString() },
            Vectors = new Vectors
            {
                Vector = new Vector { Data = { vector } }
            }
        };

        // PAYLOAD
        point.Payload.Add("text", new Value { StringValue = chunk });
        point.Payload.Add("blogId", new Value { StringValue = dto.BlogId });
        point.Payload.Add("title", new Value { StringValue = dto.Title });
        point.Payload.Add("author", new Value { StringValue = dto.AuthorName });
        point.Payload.Add("tags", new Value { StringValue = string.Join(",", dto.Tags) });
        point.Payload.Add("domain", new Value { StringValue = dto.Domain });
        point.Payload.Add("createdAt", new Value { StringValue = dto.CreatedAt.ToString("o") });
        point.Payload.Add("chunk_index", new Value { IntegerValue = index });
        point.Payload.Add("total_chunks", new Value { IntegerValue = total });

        await _qdrant.UpsertAsync(COLLECTION, new[] { point });

        Console.WriteLine($"Inserted chunk {index}/{total}");
    }

    public class EmbeddingResponse
    {
        public float[] embedding { get; set; }
    }
}
