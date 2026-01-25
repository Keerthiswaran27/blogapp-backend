using BlogApp1.Server.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IngestionService _embedService;
        private readonly RetrievalService _retrievalService;
        private readonly LlmService _llmService; // 👈

        public ChatController(
            IngestionService embedService,
            RetrievalService retrievalService,
            LlmService llmService)   // 👈
        {
            _embedService = embedService;
            _retrievalService = retrievalService;
            _llmService = llmService;
        }
        [HttpPost("ask")]
        public async Task<IActionResult> Ask([FromBody] string question)
        {
            // 1. Embed
            var queryVector =
                await _embedService.GenerateEmbedding(question);

            // 2. Retrieve chunks
            var chunks =
                await _retrievalService.RetrieveAsync(
                    question,
                    queryVector);

            // 3. Call LLM
            var answer =
                await _llmService.GenerateAnswer(
                    question,
                    chunks);

            return Ok(new
            {
                answer,
                chunks
            });
        }

    }

}
