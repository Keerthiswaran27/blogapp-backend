using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Net.Http;
using static Supabase.Postgrest.Constants;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogsController : ControllerBase
    {
        private readonly Client _supabase;

        public BlogsController(IConfiguration config)
        {
            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:Key"];

            _supabase = new Client(supabaseUrl, supabaseKey);
        }

        // GET api/blogs
        [HttpGet]
        public async Task<IActionResult> GetBlogs()
        {
            var result = await _supabase.From<BlogData>().Get();

            var dtoList = result.Models.Select(b => new BlogDto
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
                Status=b.Status,
                ViewCount = b.ViewCount,
                LikesCount = b.LikesCount,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt
            }).ToList();

            return Ok(dtoList);
        }

        // GET api/blogs/slug/{slug}
        [HttpGet("slug/{slug}")]
        public async Task<IActionResult> GetBlogBySlug(string slug)
        {
            var result = await _supabase
                .From<BlogData>()
                .Filter("slug", Operator.Equals, slug)
                .Single();

            if (result == null)
                return NotFound();

            var dto = new BlogDto
            {
                Id = result.Id,
                Title = result.Title,
                Slug = result.Slug,
                Content = result.Content,
                CoverImageUrl = result.CoverImageUrl,
                AuthorName = result.AuthorName,
                AuthorUid = result.AuthorUid,
                Domain = result.Domain,
                Tags = result.Tags,
                ViewCount = result.ViewCount,
                LikesCount = result.LikesCount,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
            };

            return Ok(dto);
        }
        [HttpGet("likedid/{uid}")]
        public async Task<IActionResult> GetLikedIdAsync(string uid)
        {
            if (string.IsNullOrEmpty(uid))
                return BadRequest("User ID is required");

            try
            {
                // ✅ Parse first
                if (!Guid.TryParse(uid, out var guid))
                    return BadRequest("Invalid User ID format");

                var response = await _supabase
                    .From<BlogUser>()
                    .Where(p => p.UserId == guid)   // 👈 now it's a direct comparison
                    .Get();

                var result = response.Models.FirstOrDefault();

                if (result == null || result.LikeId == null)
                    return Ok(new List<int>());

                return Ok(result.LikeId.ToList());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetLikedIdAsync: {ex}");
                return StatusCode(500, "Server error fetching liked IDs");
            }
        }
        [HttpGet("history/{userId:guid}")]
        public async Task<ActionResult<IEnumerable<int>>> GetReadingHistoryIds(Guid userId)
        {
            // Fetch user by user_id
            var userResponse = await _supabase
                .From<BlogUser>()
                .Where(b => b.UserId == userId)
                .Single();

            var blogUser = userResponse;
            if (blogUser == null)
                return NotFound("User not found.");

            // Return read_history as-is (empty array if null)
            var readHistoryIds = blogUser.ReadHistory ?? Array.Empty<int>();
            return Ok(readHistoryIds);
        }




        [HttpGet("savedid/{uid}")]
        public async Task<IActionResult> GetSavedIdAsync(Guid uid)
        {
            try
            {
                var response = await _supabase
                    .From<BlogUser>()
                    .Where(p => p.UserId == uid)
                    .Get();

                var result = response.Models.FirstOrDefault();

                if (result == null)
                {
                    return Ok(new List<int>());
                }

                // if SavedId is null, return empty list
                var savedIds = result.SavedId ?? Array.Empty<int>();
                return Ok(savedIds.ToList());
            }
            catch (Exception ex)
            {
                // Log error
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("following/{uid}")]
        public async Task<IActionResult> GetFollowingAsync(Guid uid)
        {
            try
            {
                var response = await _supabase
                    .From<BlogUser>()
                    .Where(p => p.UserId == uid)
                    .Get();

                var user = response.Models.FirstOrDefault();

                if (user == null || user.Following == null)
                    return Ok(new List<string>());

                return Ok(user.Following.ToList());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpGet("followers/{uid:guid}")]
        public async Task<IActionResult> GetFollowers(Guid uid)
        {
            if (uid == Guid.Empty)
                return BadRequest("Invalid UID.");

            var response = await _supabase
                .From<BlogUser>()
                .Where(x => x.UserId == uid)
                .Single();

            var user = response;
            if (user == null)
                return NotFound("User not found.");

            var followers = user.Follower.ToList() ?? new List<string>();

            return Ok(followers);
        }

        public class LikeIdRequest
        {
            public Guid Uid { get; set; }
        }
        public class LikeRequest
        {
            public int Id { get; set; }
            public bool State { get; set; }
            public Guid UserId { get; set; }
        }
        public class SaveIdRequest
        {
            public Guid Uid {  set; get; }
            public int BlogId { set; get; }
            public bool State {  set; get; }
        }
        [HttpPost("save")]
        public async Task<IActionResult> SaveId([FromBody] SaveIdRequest Model)
        {
            var result = await _supabase.From<BlogUser>().Where(p => p.UserId == Model.Uid).Single();
            if(result.SavedId==null)
            {
                if (Model.State)
                {
                    List<int> sid = new();
                    sid.Add(Model.BlogId);
                    result.SavedId = sid.ToArray();
                }
                else
                {
                    List<int> sid = result.SavedId.ToList();
                    sid.Remove(Model.BlogId);
                    result.SavedId = sid.ToArray();
                }
            }
            else
            {
                if (!result.SavedId.Contains(Model.BlogId) && Model.State)
                {
                    List<int> sid = result.SavedId.ToList();
                    sid.Add(Model.BlogId);
                    result.SavedId = sid.ToArray();
                }
                else
                {
                    List<int> sid = result.SavedId.ToList();
                    sid.Remove(Model.BlogId);
                    result.SavedId = sid.ToArray();
                }
            }

                await _supabase.From<BlogUser>().Update(result);
            return Ok(new {success = true,savedid = result.SavedId});
        }
        
        [HttpPost("like")]
        public async Task<IActionResult> LikeCount([FromBody] LikeRequest request)
        {
            var result = await _supabase.From<BlogData>()
                                        .Where(p => p.Id == request.Id)
                                        .Single();

            if (result == null)
                return NotFound("Post not found");
            var result1 = await _supabase.From<BlogUser>()
                                        .Where(p => p.UserId == request.UserId)
                                        .Single();
            // 2. Increment or Decrement like count
            
            if (request.State)
            {
                result.LikesCount += 1;
                List<int> likeid = result1.LikeId != null? result1.LikeId.ToList(): new List<int>();
                likeid.Add(request.Id);
                int[] likeid1 = likeid.ToArray();
                result1.LikeId = likeid1;
                await _supabase.From<BlogUser>().Update(result1);
            }
            else
            {
                result.LikesCount = Math.Max(0, result.LikesCount - 1);
                List<int> likeid = result1.LikeId != null ? result1.LikeId.ToList() : new List<int>();
                if(likeid.Contains(request.Id))
                {
                    likeid.Remove(request.Id);
                    int[] likeid1 = likeid.ToArray();
                    result1.LikeId = likeid1;
                    await _supabase.From<BlogUser>().Update(result1);
                }
            }
                

            // 3. Update in DB
            await _supabase.From<BlogData>().Update(result);

            return Ok(new { success = true, likes = result.LikesCount,likeid =result1.LikeId });
        }

        [HttpPost("newblog")]
        public async Task<ActionResult<BlogDto>> CreateBlog([FromBody] NewBlog1 newBlog)
        {
            if (newBlog == null)
                return BadRequest("Payload is required.");

            if (string.IsNullOrWhiteSpace(newBlog.Title) ||
                string.IsNullOrWhiteSpace(newBlog.Slug) ||
                string.IsNullOrWhiteSpace(newBlog.Content) ||
                string.IsNullOrWhiteSpace(newBlog.AuthorUid))
            {
                return BadRequest("Title, Slug, Content and AuthorUid are required.");
            }

            var now = DateTime.UtcNow;

            var model = new BlogData
            {
                // From NewBlog
                Title = newBlog.Title,
                Slug = newBlog.Slug,
                Content = newBlog.Content,
                CoverImageUrl = newBlog.CoverImageUrl,
                AuthorName = newBlog.AuthorName,
                AuthorUid = newBlog.AuthorUid,
                Tags = newBlog.Tags ?? new(),
                Domain = newBlog.Domain,
                MetaDescription = newBlog.MetaDescription,
                Summary = newBlog.Summary,
                ReadingTime = newBlog.ReadingTime,
                WordCount = newBlog.WordCount,

                // Defaults / system fields
                ViewCount = 0,
                LikesCount = 0,
                Status = "pending",
                IsPublished = false,
                SourceUrl = null,
                CreatedAt = now,
                UpdatedAt = null,
                PublishedAt = null,
                ReviewedAt = null,
                EditorUid = null
            };

            var insertResponse = await _supabase
                .From<BlogData>()
                .Insert(model); // Id is generated by DB

            var created = insertResponse.Models.Count > 0
                ? insertResponse.Models[0]
                : model;

            var result = new BlogDto
            {
                Id = created.Id,
                Title = created.Title,
                Slug = created.Slug,
                Content = created.Content,
                CoverImageUrl = created.CoverImageUrl,

                AuthorName = created.AuthorName,
                AuthorUid = created.AuthorUid,

                Tags = created.Tags ?? new(),
                Domain = created.Domain,

                ViewCount = created.ViewCount,
                LikesCount = created.LikesCount,

                Status = created.Status,
                IsPublished = created.IsPublished,

                ReadingTime = created.ReadingTime,
                WordCount = created.WordCount,

                MetaDescription = created.MetaDescription,
                Summary = created.Summary,
                SourceUrl = created.SourceUrl,

                CreatedAt = created.CreatedAt,
                UpdatedAt = created.UpdatedAt,
                PublishedAt = created.PublishedAt,
                ReviewedAt = created.ReviewedAt,

                EditorUid = null
            };

            return Ok(result);
        }

        [HttpPost("follow")]
        public async Task<IActionResult> FollowUserAsync([FromBody] FollowRequest request)
        {
            try
            {
                // 1. Get Reader (who is following)
                var readerResponse = await _supabase
                    .From<BlogUser>()
                    .Where(p => p.UserId == request.ReaderUid)
                    .Get();
                var reader = readerResponse.Models.FirstOrDefault();

                // 2. Get Author (who is being followed)
                var authorResponse = await _supabase
                    .From<BlogUser>()
                    .Where(p => p.UserId == request.AuthorUid)
                    .Get();
                var author = authorResponse.Models.FirstOrDefault();

                if (reader == null || author == null)
                    return BadRequest("Invalid user IDs.");

                // 3. Update Following list of Reader
                var updatedFollowing = (reader.Following ?? new string[0]).ToList();
                if (!updatedFollowing.Contains(request.AuthorUid.ToString()))
                    updatedFollowing.Add(request.AuthorUid.ToString());
                reader.Following = updatedFollowing.ToArray();

                await _supabase.From<BlogUser>().Upsert(reader);

                // 4. Update Followers list of Author
                var updatedFollowers = (author.Follower ?? new string[0]).ToList();
                if (!updatedFollowers.Contains(request.ReaderUid.ToString()))
                    updatedFollowers.Add(request.ReaderUid.ToString());
                author.Follower = updatedFollowers.ToArray();

                await _supabase.From<BlogUser>().Upsert(author);

                return Ok(new { Message = "Followed successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Server error: {ex.Message}");
            }
        }
        [HttpPost("saveaiblog")]
        public async Task<IActionResult> SaveAIBlog([FromBody] AIBlogDto aiBlog)
        {
            if (aiBlog == null || string.IsNullOrWhiteSpace(aiBlog.Title))
                return BadRequest("Invalid AI blog data.");

            try
            {
                

                var newBlog = new BlogData
                {
                    Title = aiBlog.Title,
                    Slug = aiBlog.Slug,
                    Content = aiBlog.Content,
                    MetaDescription = aiBlog.MetaDescription ?? "",
                    Domain = aiBlog.Category ?? "General",
                    Tags = aiBlog.Tags ?? new(),
                    AuthorName = aiBlog.AuthorName ?? "Unknown Author",
                    AuthorUid = aiBlog.AuthorUid ?? Guid.NewGuid().ToString(),
                    CoverImageUrl = aiBlog.CoverImageUrl,
                    Status = "pending",
                    ReadingTime = ParseReadingTime(aiBlog.ReadingTime),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ViewCount = 0,
                    LikesCount = 0
                };

                var response = await _supabase.From<BlogData>().Insert(newBlog);

                if (response.Models.Count > 0)
                {
                    return Ok(new
                    {
                        message = "✅ AI Blog saved successfully!",
                        blogId = response.Models.First().Id,
                        slug = aiBlog.Slug,
                        title = newBlog.Title
                    });
                }

                return StatusCode(500, new { error = "Failed to save the blog into Supabase." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An error occurred while saving the AI blog.",
                    details = ex.Message
                });
            }
        }

        private static long? ParseReadingTime(string readingTime)
        {
            if (string.IsNullOrWhiteSpace(readingTime)) return null;
            var parts = readingTime.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (long.TryParse(parts[0], out var minutes))
                return minutes;
            return null;
        }

        public class FollowRequest
        {
            public Guid ReaderUid { get; set; }  // the person who follows
            public Guid AuthorUid { get; set; }  // the person being followed
        }

        private static long CalculateReadingTime(string content)
        {
            // simple reading time estimate: 200 words per minute
            if (string.IsNullOrWhiteSpace(content))
                return 0;

            var wordCount = content.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
            return (long)Math.Ceiling(wordCount / 150.0);
        }   
        public class UploadResponse
        {
            public Guid Url { get; set; }
        }
        [HttpPost("read-history")]
        public async Task<IActionResult> TrackRead([FromBody] TrackReadRequest request)
        {
            if (request == null || request.UserId == Guid.Empty)
                return BadRequest("Invalid request.");

            // 1. Get the user by user_id
            var user = await _supabase
                .From<BlogUser>()
                .Where(u => u.UserId == request.UserId)
                .Single();

            if (user == null)
                return NotFound("User not found.");

            // 2. Check if blog already in read history
            var history = user.ReadHistory ?? Array.Empty<int>();

            if (history.Contains(request.BlogId))
            {
                // Already counted; return current state without changing anything
                return Ok(new
                {
                    alreadyRead = true,
                    userId = user.UserId,
                    blogId = request.BlogId
                });
            }

            // 3. Add blog id to read history
            var updatedHistory = history.ToList();
            updatedHistory.Add(request.BlogId);
            user.ReadHistory = updatedHistory.ToArray();
            user.UpdatedAt = DateTime.UtcNow;

            // 4. Load blog and increment view count
            var blog = await _supabase
                .From<BlogData>()
                .Where(b => b.Id == request.BlogId)
                .Single();

            if (blog == null)
                return NotFound("Blog not found.");

            blog.ViewCount += 1;
            blog.UpdatedAt = DateTime.UtcNow;

            // 5. Persist both updates
            await user.Update<BlogUser>();
            await blog.Update<BlogData>();

            return Ok(new
            {
                alreadyRead = false,
                userId = user.UserId,
                blogId = request.BlogId,
                newViewCount = blog.ViewCount,
                newReadHistory = user.ReadHistory
            });
        }
        [HttpGet("author-blogs/{authorUid}")]
        public async Task<ActionResult<List<BlogDto>>> GetByAuthor(string authorUid)
        {
            if (string.IsNullOrWhiteSpace(authorUid))
                return BadRequest("Author UID is required.");

            var response = await _supabase
                .From<BlogData>()
                .Filter(b => b.AuthorUid, Operator.Equals, authorUid)
                .Get();

            var blogs = response.Models;

            var result = blogs.Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                Content = b.Content,
                CoverImageUrl = b.CoverImageUrl,

                AuthorName = b.AuthorName,
                AuthorUid = b.AuthorUid,              // string → string, no Guid.Parse

                Tags = b.Tags,
                Domain = b.Domain,

                ViewCount = b.ViewCount,
                LikesCount = b.LikesCount,

                Status = b.Status,
                IsPublished = b.IsPublished,

                ReadingTime = b.ReadingTime,
                WordCount = b.WordCount,

                MetaDescription = b.MetaDescription,
                Summary = b.Summary,
                SourceUrl = b.SourceUrl,

                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                PublishedAt = b.PublishedAt,
                ReviewedAt = b.ReviewedAt,

                // BlogData.EditorUid is string? in your earlier model, but BlogDto.EditorUid is Guid?
                // Only parse when not null/empty
                EditorUid = string.IsNullOrWhiteSpace(b.EditorUid)
                    ? null
                    : Guid.Parse(b.EditorUid)
            }).ToList();

            return Ok(result);
        }

    }
}
