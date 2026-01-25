using BlogApp1.Shared;
using System.Text;
using System.Text.Json;
namespace BlogApp1.Server.Services
{
    public class LlmService
    {
        private readonly HttpClient _http;

        private const string GEMINI_API =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=AIzaSyAiqeRrkw1w41IittLen7cv6vtMDsWH-38";

        public LlmService()
        {
            _http = new HttpClient();
        }

        public async Task<string> GenerateAnswer(
    string question,
    List<RagChunkResult> chunks)
        {
            var context = string.Join("\n\n",
                chunks.Select(c => c.Text));

            var prompt = $@"
You are a helpful AI assistant.
Answer ONLY using the given context.

Context:
{context}

Question:
{question}

Answer:";

            var body = new
            {
                contents = new[]
                {
            new
            {
                parts = new[]
                {
                    new { text = prompt }
                }
            }
        }
            };

            var json = JsonSerializer.Serialize(body);

            var response = await _http.PostAsync(
                GEMINI_API,
                new StringContent(json,
                Encoding.UTF8,
                "application/json"));

            var result =
                await response.Content.ReadAsStringAsync();

            // 🔥 PARSE ONLY TEXT
            return ExtractText(result);
        }
        private string ExtractText(string json)
        {
            using var doc = JsonDocument.Parse(json);

            var root = doc.RootElement;

            var text = root
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return text;
        }

    }

}
