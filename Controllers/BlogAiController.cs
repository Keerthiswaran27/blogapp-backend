using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;
using Qdrant.Client.Grpc;
using System.Text;
using System.Text.Json;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/blog-ai")]
    public class BlogAiController : ControllerBase
    {
        private readonly HttpClient _http;

        private const string GEMINI_API_KEY = "AIzaSyBT4UFf1dEz_AhNBNXO26e_PEGfacBTq8k";

        // Use stable model
        private const string MODEL =
            "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key=";

        public BlogAiController()
        {
            _http = new HttpClient();
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateBlog(
            [FromBody] BlogGenerateRequest request)
        {
            try
            {
                string prompt = BuildPrompt(request);

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

                var content = new StringContent(
                    json,
                    Encoding.UTF8,
                    "application/json");

                var response = await _http.PostAsync(
                    MODEL + GEMINI_API_KEY,
                    content);

                var raw = await response.Content.ReadAsStringAsync();

                // DEBUG (temporary)
                Console.WriteLine("===== RAW GEMINI RESPONSE =====");
                Console.WriteLine(raw);
                Console.WriteLine("================================");

                var blog = ExtractAndParseBlog(raw);
                if (string.IsNullOrWhiteSpace(blog.ImageUrl))
                {
                    blog.ImageUrl =
                        $"https://source.unsplash.com/1600x900/?{request.Topic.Replace(" ", ",")}";
                }
                return Ok(blog);
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    error = ex.Message
                });
            }
        }

        // ================= PROMPT ===================
        private string BuildPrompt(BlogGenerateRequest req)
        {
            var prompt = $@"
            You are an expert SEO blog writer.

            Write a {req.WordCount} word blog.

            Topic: {req.Topic}
            Tone: {req.Tone}

            Return ONLY valid JSON in this format:

            {{
              ""title"": ""..."",
              ""slug"": ""..."",
              ""content"": ""HTML blog using <h1>, <h2>, <p>, <ul> tags"",
              ""tags"": [""tag1"", ""tag2""],
              ""domain"": ""category"",
              ""meta_description"": ""SEO description (max 160 chars)"",
              ""summary"": ""Short summary"",
              ""imageUrl"": ""REQUIRED: Public stock image URL related to topic""
            }}

            STRICT RULES:
            - imageUrl MUST NOT be empty
            - MUST be a real image link
            - MUST start with https://
            - MUST be from:
               • images.unsplash.com
               • pexels.com
               • pixabay.com
            - If you cannot find image, GENERATE a relevant Unsplash image URL
            - DO NOT return empty string
            - Content MUST be HTML
            - Slug lowercase-hyphen
            - No markdown
            - No explanation
            - ONLY JSON
            ";

            return prompt;
        }




        private BlogGenerateResponse ExtractAndParseBlog(string raw)
        {
            using var doc = JsonDocument.Parse(raw);

            var text =
                doc.RootElement
                   .GetProperty("candidates")[0]
                   .GetProperty("content")
                   .GetProperty("parts")[0]
                   .GetProperty("text")
                   .GetString();

            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Gemini returned empty response");

            text = text.Replace("```json", "")
                       .Replace("```", "")
                       .Trim();

            return JsonSerializer.Deserialize<BlogGenerateResponse>(
                text,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
        }


    }
}
