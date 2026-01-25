using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using static Supabase.Postgrest.Constants;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentsController : ControllerBase
    {
        private readonly Client _supabase;

        public CommentsController(IConfiguration config)
        {
            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:Key"];

            _supabase = new Client(supabaseUrl, supabaseKey);
        }

        [HttpGet("{blogId}")]
        public async Task<IActionResult> GetCommentsByBlogId(int blogId)
        {
            try
            {
                var response = await _supabase
                    .From<BlogComment>()
                    .Where(x => x.BlogId == blogId)
                    .Get();

                var comments = response.Models
                    .Select(c => new CommentDto
                    {
                        CommentUid = c.CommentUid,
                        BlogId = c.BlogId,
                        CommentUserId = c.CommentUserId,
                        ParentCommentUid = c.ParentCommentUid,
                        IsParent = c.IsParent,
                        Content = c.Content,
                        LikesCount = c.LikesCount,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        AuthorName = c.AuthorName
                    })
                    .ToList();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        [HttpGet("user-info/{uid}")]
        public async Task<ActionResult<string>> GetUserName(Guid uid)
        {
         

            var user = await _supabase.From<BlogComment>()
                .Where(u => u.CommentUid == uid)
                .Single();

            if (user == null)
                return NotFound("User not found");

            return Ok(user.AuthorName);
        }

        [HttpGet("authorname/{id}")]
        public async Task<ActionResult<string>> GetAuthorName(int id)
        {
            var user = await _supabase.From<BlogData>()
                .Where(u => u.Id == id).Single();

            if (user == null || string.IsNullOrEmpty(user.AuthorName))
                return NotFound("User not found");

            return Ok(user.AuthorName);
        }
        [HttpGet("gettitle/{id}")]
        public async Task<ActionResult<string>> GetBlogTitle(int id)
        {
            var user = await _supabase.From<BlogData>()
                .Where(u => u.Id == id).Single();

            if (user == null || string.IsNullOrEmpty(user.Title))
                return NotFound("User not found");

            return Ok(user.Title);
        }

        [HttpGet("recentcomments")]
        public async Task<IActionResult> GetAllComments()
        {
            try
            {
                var response = await _supabase
                    .From<BlogComment>()
                    .Order(x => x.CreatedAt, Ordering.Descending) // order by latest first
                    .Limit(3) // take only 4
                    .Get();

                var comments = response.Models
                    .Select(c => new CommentDto
                    {
                        CommentUid = c.CommentUid,
                        BlogId = c.BlogId,
                        CommentUserId = c.CommentUserId,
                        ParentCommentUid = c.ParentCommentUid,
                        IsParent = c.IsParent,
                        Content = c.Content,
                        LikesCount = c.LikesCount,
                        CreatedAt = c.CreatedAt,
                        UpdatedAt = c.UpdatedAt,
                        AuthorName = c.AuthorName
                    })
                    .ToList();

                return Ok(comments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddComment([FromBody] CommentDto commentDto)
        {
            try
            {
                var newComment = new BlogComment
                {
                    CommentUid= commentDto.CommentUid,
                    BlogId = commentDto.BlogId,
                    CommentUserId = commentDto.CommentUserId,
                    ParentCommentUid = commentDto.ParentCommentUid, // null if parent
                    IsParent = commentDto.ParentCommentUid == null,
                    Content = commentDto.Content,
                    LikesCount = 0,
                    CreatedAt = DateTime.UtcNow,
                    AuthorName = commentDto.AuthorName
                };

                var response = await _supabase.From<BlogComment>().Insert(newComment);

                if (response.Models.Any())
                {
                    // Map to DTO before returning
                    var added = response.Models.First();
                    var resultDto = new CommentDto
                    {
                        BlogId = added.BlogId,
                        CommentUserId = added.CommentUserId,
                        ParentCommentUid = added.ParentCommentUid,
                        IsParent = added.IsParent,
                        Content = added.Content,
                        LikesCount = added.LikesCount,
                        CreatedAt = added.CreatedAt,
                        UpdatedAt = added.UpdatedAt,
                        AuthorName= added.AuthorName
                    };
                    return Ok(resultDto);
                }

                return BadRequest("Unable to add comment.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
        // GET: api/Comments/by-user/{userId}
        [HttpGet("by-user/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<CommentDto>>> GetCommentsByUser(Guid userId)
        {
            // Filter by comment_user_id = userId
            var response = await _supabase
                .From<BlogComment>()
                .Filter("comment_user_id", Operator.Equals, userId.ToString()) // guid as string [web:87][web:74]
                .Get();

            var comments = response.Models;

            var result = comments.Select(c => new CommentDto
            {
                CommentUid = c.CommentUid,
                BlogId = c.BlogId,
                CommentUserId = c.CommentUserId,
                ParentCommentUid = c.ParentCommentUid,
                IsParent = c.IsParent,
                Content = c.Content,
                LikesCount = c.LikesCount,
                CreatedAt = c.CreatedAt,
                UpdatedAt = c.UpdatedAt,
                AuthorName = c.AuthorName,
                BlogTitle = null // fill later if you join with blog_data
            }).ToList();

            return Ok(result);
        }

    }
}
