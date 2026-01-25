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
    public class AdminBlogsController : ControllerBase
    {
        private readonly Client _supabase;

        // ✅ Constructor with config-based initialization
        public AdminBlogsController(IConfiguration config)
        {
            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:Key"];

            _supabase = new Client(supabaseUrl, supabaseKey);
        }

        // ✅ Get all blogs (with optional filters later if needed)
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BlogDto>>> GetAllBlogs()
        {
            var response = await _supabase
                .From<BlogData>()
                .Get();

            var blogs = response.Models
                .Select(b => new BlogDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Slug = b.Slug,
                    Content = b.Content,
                    CoverImageUrl = b.CoverImageUrl,
                    AuthorName = b.AuthorName,
                    AuthorUid = b.AuthorUid,
                    Domain = b.Domain,
                    Tags = b.Tags,
                    ViewCount = b.ViewCount,
                    LikesCount = b.LikesCount,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
                    UpdatedAt = b.UpdatedAt
                }).ToList();

            return Ok(blogs);
        }

        // ✅ Get single blog by Id
        [HttpGet("{id}")]
        public async Task<ActionResult<BlogDto>> GetBlogById(int id)
        {
            var response = await _supabase
                .From<BlogData>()
                .Where(b => b.Id == id)
                .Single();

            if (response == null) return NotFound();

            return Ok(new BlogDto
            {
                Id = response.Id,
                Title = response.Title,
                Slug = response.Slug,
                Content = response.Content,
                CoverImageUrl = response.CoverImageUrl,
                AuthorName = response.AuthorName,
                AuthorUid = response.AuthorUid,
                Domain = response.Domain,
                Tags = response.Tags,
                ViewCount = response.ViewCount,
                LikesCount = response.LikesCount,
                Status = response.Status,
                CreatedAt = response.CreatedAt,
                UpdatedAt = response.UpdatedAt
            });
        }

        // ✅ Delete a blog by Id
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBlog(int id)
        {
            var blog = new BlogData { Id = id };
            await _supabase.From<BlogData>().Delete(blog);

            return NoContent();
        }
    }
}
