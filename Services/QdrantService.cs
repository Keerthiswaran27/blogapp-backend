using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace BlogApp1.Server.Services
{
    public class QdrantService
    {
        private readonly HttpClient _http;
        private const string QDRANT_URL = "YOUR_CLUSTER_URL";
        private const string API_KEY = "YOUR_API_KEY";
        private const string COLLECTION = "rag_blog_chunks";

        public QdrantService()
        {
            _http = new HttpClient();
            _http.DefaultRequestHeaders.Add("api-key", API_KEY);
        }

        public async Task InsertVector(float[] vector, string text)
        {
            var body = new
            {
                points = new[]
                {
                new
                {
                    id = Guid.NewGuid().ToString(),
                    vector = vector,
                    payload = new
                    {
                        text = text,
                        source = "test",
                        domain = "blog"
                    }
                }
            }
            };

            var json = JsonSerializer.Serialize(body);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"{QDRANT_URL}/collections/{COLLECTION}/points?wait=true";

            var response = await _http.PutAsync(url, content);
            var result = await response.Content.ReadAsStringAsync();

            Console.WriteLine(result);
        }
    }
}
