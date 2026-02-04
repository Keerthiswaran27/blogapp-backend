using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

[ApiController]
[Route("api/article-generator")]
public class ArticleGeneratorController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ArticleGeneratorController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateArticle([FromBody] ArticleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Topic))
            return BadRequest("Topic is required");

        var client = _httpClientFactory.CreateClient();

        var prompt = $@"Write a blog post about: {request.Topic}

Tone: {request.Tone ?? "professional"}
Length: 800 words
Format: Markdown with headings

Write the complete article:";

        var body = new
        {
            inputs = prompt,
            parameters = new
            {
                max_new_tokens = 2000,
                temperature = 0.7,
                return_full_text = false
            }
        };

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // FREE Hugging Face Inference (works without key for many models)
        var response = await client.PostAsync(
            "https://api-inference.huggingface.co/models/microsoft/DialoGPT-large",
            content);

        var responseString = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return StatusCode((int)response.StatusCode, responseString);

        using var doc = JsonDocument.Parse(responseString);
        var generatedText = doc.RootElement[0].GetProperty("generated_text").GetString();

        return Ok(new
        {
            success = true,
            content = generatedText,
            wordCount = generatedText?.Split().Length ?? 0
        });
    }
}

public class ArticleRequest
{
    public string Topic { get; set; } = string.Empty;
    public string? Tone { get; set; }
}
