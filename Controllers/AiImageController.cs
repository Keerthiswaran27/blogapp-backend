using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/ai-image")]
public class AiImageController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public AiImageController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateImage([FromBody] ImagePromptRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return BadRequest("Prompt is required");

        var apiKey = _configuration["Gemini:ApiKey"];
        var client = _httpClientFactory.CreateClient();

        // Multiple model fallback strategy for quota issues
        var models = new[]
        {
            "gemini-2.5-flash-image",
            "gemini-2.0-flash-image", 
            "gemini-2.5-flash-image-preview"
        };

        foreach (var model in models)
        {
            try
            {
                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

                var body = new
                {
                    contents = new[]
                    {
                        new
                        {
                            role = "user",
                            parts = new[] { new { text = request.Prompt } }
                        }
                    },
                    generationConfig = new
                    {
                        responseModalities = new[] { "TEXT", "IMAGE" },
                        temperature = 0.7
                    }
                };

                var json = JsonSerializer.Serialize(body, new JsonSerializerOptions 
                { 
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase 
                });
                
                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, httpContent);
                var responseString = await response.Content.ReadAsStringAsync();

                // FIXED: Use (int)response.StatusCode == 429
                if ((int)response.StatusCode == 429) // Quota exceeded, try next model
                    continue;

                if (!response.IsSuccessStatusCode)
                    continue;

                // Parse image response
                using var doc = JsonDocument.Parse(responseString);
                if (TryExtractImage(doc, out var imageBytes))
                {
                    return File(imageBytes, "image/png");
                }
            }
            catch
            {
                continue; // Try next model
            }
        }

        return StatusCode(429, "All image models quota exceeded. Check https://ai.dev/rate-limit or enable billing for Tier 1 access.");
    }

    private static bool TryExtractImage(JsonDocument doc, out byte[] imageBytes)
    {
        imageBytes = null;
        
        try
        {
            if (!doc.RootElement.TryGetProperty("candidates", out var candidates) ||
                candidates.GetArrayLength() == 0)
                return false;

            var firstCandidate = candidates[0];
            if (!firstCandidate.TryGetProperty("content", out var content) ||
                !content.TryGetProperty("parts", out var parts))
                return false;

            foreach (var part in parts.EnumerateArray())
            {
                if (part.TryGetProperty("inlineData", out var inlineData) &&
                    inlineData.TryGetProperty("data", out var dataProp) &&
                    dataProp.ValueKind == JsonValueKind.String)
                {
                    var base64Data = dataProp.GetString();
                    if (!string.IsNullOrEmpty(base64Data))
                    {
                        imageBytes = Convert.FromBase64String(base64Data);
                        return true;
                    }
                }
            }
        }
        catch
        {
            // Ignore parsing errors
        }
        
        return false;
    }
}

public class ImagePromptRequest
{
    public string Prompt { get; set; } = string.Empty;
}
