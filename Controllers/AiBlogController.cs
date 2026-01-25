using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Net.Http.Json;
using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AiBlogController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AiBlogController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateBlog(
    [FromBody] OllamaBlogRequest request)
    {
        var client = _httpClientFactory.CreateClient("Ollama");

        var prompt =
            $"You are a professional blog writer.\n" +
            $"Write an SEO-friendly blog post about: \"{request.Topic}\".\n" +
            $"Target length: around {request.WordCount} words.\n" +
            $"Use headings, short paragraphs, and a friendly tone.";

        var body = new
        {
            model = "phi3",
            prompt,
            stream = false   // ❌ No streaming
        };

        var resp = await client.PostAsJsonAsync(
            "/api/generate",
            body);

        if (!resp.IsSuccessStatusCode)
        {
            var err = await resp.Content.ReadAsStringAsync();
            return BadRequest(err);
        }

        var raw = await resp.Content.ReadAsStringAsync();

        // Ollama response structure
        var json = JsonDocument.Parse(raw);
        var text = json.RootElement
                       .GetProperty("response")
                       .GetString();

        return Ok(new
        {
            blog = text
        });
    }


    private sealed class OllamaStreamChunk
    {
        public string Response { get; set; } = string.Empty;
        public bool Done { get; set; }
    }
}
