using BlogApp1.Server.Services;
using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/rag")]
    public class RagIngestionController : ControllerBase
    {
        private readonly IngestionService _service;

        public RagIngestionController(IngestionService service)
        {
            _service = service;
        }

        [HttpPost("ingest-blog")]
        public async Task<IActionResult> Ingest(RagIngestionDto dto)
        {
            // 1. Clean HTML
            var clean = _service.ParseHtml(dto.HtmlContent);

            // 2. Chunk
            var chunks = _service.ChunkText(clean);

            Console.WriteLine($"TOTAL CHUNKS: {chunks.Count}");

            // 3. Embed + Insert
            for (int i = 0; i < chunks.Count; i++)
            {
                var chunk = chunks[i];

                var vector = await _service.GenerateEmbedding(chunk);

                Console.WriteLine($"VECTOR LENGTH: {vector.Length}");

                await _service.InsertIntoQdrant(
                    vector,
                    chunk,
                    dto,
                    i + 1,            // chunk index
                    chunks.Count     // total chunks
                );
            }

            return Ok("Ingestion completed successfully");
        }
    }
}
