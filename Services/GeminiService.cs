using System.Net.Http;
using System.Text;
using System.Text.Json;
using BlogApp1.Shared;

namespace BlogApp1.Server.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;  // ✅ FIELD
        private readonly string _apiKey;          // ✅ FIELD

        public GeminiService(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;  // ✅ ASSIGN
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Missing Gemini:ApiKey");
        }

        public async Task<string> GenerateAnswerAsync(string question, List<ChunkResult> chunks)
        {
            if (!chunks.Any())
                return "No relevant information found in knowledge base.";

            // 🔥 STEP 1: Build context from chunks (like your blog inputs)
            var context = string.Join("\n\n", chunks.OrderByDescending(c => c.Score).Take(4)
                .Select((chunk, index) =>
                    $"SOURCE {index + 1}:\n" +
                    $"Title: {chunk.Title}\n" +
                    $"Content: {chunk.Content}\n" +
                    $"Author: {chunk.Author}\n" +
                    $"Score: {chunk.Score:F3}"
                ));

            // 🔥 STEP 2: PERFECT RAG PROMPT (like your blog prompt)
            string prompt = $@"
You are an expert AI assistant who answers questions using ONLY provided sources.

Generate a concise, accurate answer in natural language.

Base your answer on these inputs:
- Question: {question}
- Sources: {context}

Strict rules:
1. Use ONLY information from SOURCES above
2. Cite sources after each fact: [1], [2], etc.
3. If answer not in sources: 'I don't have enough information from sources.'
4. Answer in 3-5 sentences maximum
5. End with 'Sources used: [1], [2]' list
6. Output ONLY plain text (no JSON, no markdown fences)

ANSWER:
";

            // 🔥 STEP 3: EXACT SAME API CALL as your GenerateBlog
            var requestBody = new
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

            var response = await _httpClient.PostAsJsonAsync(
               $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                requestBody
            );

            // 🔥 STEP 4: EXACT SAME ERROR HANDLING as your GenerateBlog
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Gemini Error: {response.StatusCode} - {error}");
                return CreateFallbackAnswer(question, chunks);
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(jsonResponse);

            // 🔥 STEP 5: EXACT SAME JSON PARSING as your GenerateBlog
            try
            {
                string contentText = doc.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString();

                // Clean any markdown fences (like your ```
                string cleaned = contentText
                    .Replace("```", "")
                    .Replace("json", "")
                    .Trim();

                return cleaned.Length > 0 ? cleaned : CreateFallbackAnswer(question, chunks);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Parse Error: {ex.Message}");
                return CreateFallbackAnswer(question, chunks);
            }
        }

        // 🔥 SAME FALLBACK as before
        private string CreateFallbackAnswer(string question, List<ChunkResult> chunks)
        {
            var topChunks = chunks.OrderByDescending(c => c.Score).Take(3).ToList();
            var answer = $"**Q: {question}**\n\n";

            for (int i = 0; i < topChunks.Count; i++)
            {
                answer += $"**[{i + 1}]** {topChunks[i].Title}\n";
                answer += $"{topChunks[i].Content.Substring(0, 150)}...\n\n";
            }

            return answer;
        }





    }
}
