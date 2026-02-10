using BlogApp1.Shared;
using System.Text;
using System.Text.Json;

namespace BlogApp1.Server.Services
{
    public class LlmService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public LlmService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> GenerateAnswer(
            string question,
            List<RagChunkResult> chunks)
        {
            try
            {
                var apiKey = _config["Gemini:ApiKey2"];

                var endpoint =
                    $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={apiKey}";

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
                    endpoint,
                    new StringContent(json,
                    Encoding.UTF8,
                    "application/json"));

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    return $"LLM API Error: {error}";
                }

                var result =
                    await response.Content.ReadAsStringAsync();

                return ExtractText(result);
            }
            catch (Exception ex)
            {
                return $"LLM Exception: {ex.Message}";
            }
        }

        private string ExtractText(string json)
        {
            try
            {
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // 🔥 Check if error exists (Gemini error response)
                if (root.TryGetProperty("error", out var error))
                {
                    var message = error.GetProperty("message").GetString();
                    return $"LLM Error: {message}";
                }

                if (!root.TryGetProperty("candidates", out var candidates))
                    return "No candidates returned.";

                if (candidates.GetArrayLength() == 0)
                    return "Empty response from LLM.";

                var firstCandidate = candidates[0];

                if (!firstCandidate.TryGetProperty("content", out var content))
                    return "No content in response.";

                if (!content.TryGetProperty("parts", out var parts))
                    return "No parts in response.";

                if (parts.GetArrayLength() == 0)
                    return "Empty parts in response.";

                var text = parts[0].GetProperty("text").GetString();

                return text ?? "No text generated.";
            }
            catch (Exception ex)
            {
                return $"Failed to parse LLM response: {ex.Message}";
            }
        }

    }
}
