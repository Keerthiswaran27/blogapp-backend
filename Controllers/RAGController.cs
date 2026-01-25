using BlogApp1.Server.Services;
using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RAGController : ControllerBase
    {
        private readonly RAGIngestionService _ragService;
        private readonly RAGQueryService _queryService;
        private readonly GeminiService _geminiService;
        public RAGController(RAGIngestionService ingestion, RAGQueryService query, GeminiService gemini)
        {
            _ragService = ingestion;
            _queryService = query;
            _geminiService = gemini;  // ✅ NEW!
        }

        [HttpPost("ingest")]
        public async Task<ActionResult<IngestionResult>> IngestBlog([FromBody] BlogIngestionRequest request)
        {
            var result = await _ragService.IngestBlogAsync(request);
            return Ok(result);
        }

        [HttpPost("query")]
        public async Task<ActionResult<QueryResult>> Query([FromBody] QueryRequest request)
        => Ok(await _queryService.QueryAsync(request));

        [HttpPost("chat")]
        public async Task<ActionResult<ChatResponse>> Chat([FromBody] ChatRequest request)
        {
            // 1. Retrieve relevant chunks
            var queryResult = await _queryService.QueryAsync(new QueryRequest { Question = request.Question });

            // 2. Generate answer with numbered sources
            var answer = await _geminiService.GenerateAnswerAsync(request.Question, queryResult.RelevantChunks);

            // 3. Track sources used (top 4)
            var sources = queryResult.RelevantChunks.Take(4).Select((c, index) => new Source
            {
                Title = c.Title,
                Url = c.Url,
                Score = c.Score,
                SourceNumber = index + 1  // [1], [2], [3], [4]
            }).ToList();

            return Ok(new ChatResponse
            {
                Answer = answer,
                Sources = sources,
                SourcesSummary = $"Used {sources.Count} sources from your knowledge base"
            });
        }

    }
}
    