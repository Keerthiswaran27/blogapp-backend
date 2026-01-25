using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeminiController : ControllerBase
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey = "";

        public GeminiController(HttpClient httpClient, IConfiguration config)
        {
            _httpClient = httpClient;
            _apiKey = config["Gemini:ApiKey"] ?? throw new ArgumentNullException("Missing Gemini:ApiKey");
        }

        [HttpPost("generate")]
        public async Task<IActionResult> GenerateBlog([FromBody] GeminiRequest input)
        {
            if (input == null || string.IsNullOrWhiteSpace(input.Title))
                return BadRequest("Invalid input");

            string prompt = $@"
You are an expert AI blog writer who creates well-structured, SEO-friendly, natural-flowing articles.

Generate a complete blog article in **HTML format** using only these tags: 
<p>, <h2>, <h3>, <strong>, <em>, and <a>.
Avoid using <ul>, <ol>, <li>, or any bullet points.

Base your article on the following inputs:

- Title: {input.Title}
- Audience: {input.Audience}
- Tone: {input.Tone}
- Length: {input.Length}
- Include Image: {(input.IncludeImage ? "true" : "false")}

Additional Author Instructions:
{input.CustomPrompt}

Follow these strict rules:

1. Write the blog in a smooth, paragraph-based narrative. Avoid lists or bullet formatting.
2. Maintain a natural flow: introduction, 2–3 body sections, and a conclusion.
3. Use only simple HTML tags (<p>, <h2>, <h3>, <strong>, <em>, <a>) for readability.
4. If 'Include Image' is true, provide a suitable **cover image related to the blog topic** that can be sourced from the **Picsum website** (https://picsum.photos). 
   Do not include any URLs — only describe what the image should represent, so a developer can later fetch it from Picsum.
5. Include an estimated_read_time based on 200 words per minute.
6. Output MUST be valid JSON in this structure:
{{
  ""title"": """",
  ""slug"": """",
  ""meta_description"": """",
  ""category"": """",
  ""tags"": ["", "", "", ""],
  ""content"": ""<p>HTML formatted blog content here</p>"",
  ""image_description"": ""Description of the image topic (for Picsum)"",
  ""estimated_read_time"": """"
}}
7. Do NOT include markdown formatting, code fences, or text outside the JSON object.
";




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
                $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}",
                requestBody
            );

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                var status = (int)response.StatusCode;

                if (status == 429)
                    return StatusCode(429, new { error = "Gemini API rate limit exceeded. Please wait and retry later.", details = error });

                return StatusCode(status, new { error = "Gemini API error occurred.", details = error });
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(jsonResponse);
            string contentText = doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            // Parse Gemini output safely
            try
            {
                // Clean markdown fences like ```json ... ```
                string cleaned = contentText
                    .Replace("```json", "")
                    .Replace("```", "")
                    .Trim();

                var blogData = JsonSerializer.Deserialize<GeminiBlogOutput>(cleaned,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (blogData == null)
                    throw new JsonException("Failed to parse Gemini JSON.");

                return Ok(blogData);
            }
            catch (Exception ex)
            {
                return Ok(new
                {
                    message = "Could not parse AI response properly. Returning raw text for debugging.",
                    error = ex.Message,
                    raw_output = contentText
                });
            }
        }
    }

    // Input model
    public class GeminiRequest
    {
        public string Title { get; set; } = "";
        public string Audience { get; set; } = "";
        public string Tone { get; set; } = "";
        public string Length { get; set; } = "";
        public string CustomPrompt { get; set; } = ""; // ✅ replaced keywords
        public bool IncludeImage { get; set; }
    }


    // Output model
    public class GeminiBlogOutput
    {
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Meta_Description { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
        public string Content { get; set; }
        public string Image_Url { get; set; }
        public string Estimated_Read_Time { get; set; }
    }
}
