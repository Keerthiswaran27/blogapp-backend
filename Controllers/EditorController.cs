using BlogApp1.Shared;

using BlogApp1.Shared.EditorModels;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using static Supabase.Postgrest.Constants;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditorController : ControllerBase
    {
        private readonly Client _supabase;

        public EditorController(IConfiguration config)
        {
            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:Key"];
            _supabase = new Client(supabaseUrl, supabaseKey);
        }

        // ==============================
        // 1️⃣ GET: api/editor/blogs?status=pending
        // ==============================
        [HttpGet("blogs")]
        public async Task<IActionResult> GetBlogs([FromQuery] string status = "pending")
        {
            var result = await _supabase.From<BlogData>()
                                        .Where(b => b.Status == status)
                                        .Get();

            var response = result.Models.Select(b => new BlogSummaryModel
            {
                Id = b.Id,
                Title = b.Title,
                AuthorName = b.AuthorName,
                Domain = b.Domain,
                Status = b.Status,
                CreatedAt = b.CreatedAt,
                ReviewedAt = b.ReviewedAt,
                EditorUid = b.EditorUid
            }).ToList();

            return Ok(response);
        }

        // ==============================
        // 2️⃣ GET: api/editor/blog/{id}
        // ==============================
        [HttpGet("blog/{id}")]
        public async Task<IActionResult> GetBlog(int id)
        {
            var result = await _supabase.From<BlogData>()
                                        .Where(b => b.Id == id)
                                        .Single();

            if (result == null)
                return NotFound();

            var b = result;

            var detail = new BlogDetailModel
            {
                Id = b.Id,
                Title = b.Title,
                Content = b.Content,
                AuthorName = b.AuthorName,
                AuthorUid = b.AuthorUid,
                Domain = b.Domain,
                Tags = b.Tags,
                CoverImageUrl = b.CoverImageUrl,
                Status = b.Status,
                //ReviewComments = b.ReviewComments,
                //RejectionReason = b.RejectionReason,
                EditorUid = b.EditorUid,
                CreatedAt = b.CreatedAt,
                UpdatedAt = b.UpdatedAt,
                ReviewedAt = b.ReviewedAt,
                Slug = b.Slug,
                MetaDescription = b.MetaDescription,
                ReadingTime = b.ReadingTime
                
            };

            return Ok(detail);
        }
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveBlog([FromBody] ApproveBlogRequest request)
        {
            await _supabase
                .From<BlogData>()
                .Where(b => b.Id == request.BlogId)
                .Set(b => b.Status!, "approved")
                .Set(b => b.EditorUid!, request.EditorUid)
                .Set(b => b.ReviewedAt!, DateTime.UtcNow)
                .Update();

            return Ok();
        }

        [HttpPost("revise")]
        public async Task<IActionResult> ReviseBlog([FromBody] ReviseBlogRequest request)
        {
            var blogResponse = await _supabase
                .From<BlogData>()
                .Where(b => b.Id == request.BlogId)
                .Get();

            var blog = blogResponse.Models.FirstOrDefault();
            if (blog == null)
                return NotFound("Blog not found");

            if (string.IsNullOrWhiteSpace(blog.AuthorUid))
                return BadRequest("Blog has no author. Cannot add editor feedback.");

            // Update blog
            await _supabase
                .From<BlogData>()
                .Where(b => b.Id == request.BlogId)
                .Set(b => b.Status!, "revise")
                .Set(b => b.EditorUid!, request.EditorUid)
                .Set(b => b.ReviewedAt!, DateTime.UtcNow)
                .Update();

            // Insert feedback
            await _supabase
                .From<EditorFeedback>()
                .Insert(new EditorFeedback
                {
                    BlogId = request.BlogId,
                    AuthorUid = blog.AuthorUid,   // ✅ guaranteed non-null now
                    EditorUid = request.EditorUid,
                    FeedbackType = "revise",
                    Message = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsAuthorVisible = true,
                    EditorName  = request.EditorName
                });

            return Ok();
        }
        [HttpGet("feedback")]
        public async Task<IActionResult> GetAllEditorFeedback()
        {
            var response = await _supabase
                .From<EditorFeedback>()
                .Get();

            var feedbackDtos = response.Models.Select(feedback => new EditorFeedbackDto
            {
                Id = feedback.Id,
                BlogId = feedback.BlogId,
                AuthorUid = feedback.AuthorUid ?? string.Empty,
                EditorUid = feedback.EditorUid ?? string.Empty,
                FeedbackType = feedback.FeedbackType,
                Message = feedback.Message ?? string.Empty,
                CreatedAt = feedback.CreatedAt,
                IsAuthorVisible = feedback.IsAuthorVisible,
                EditorName = feedback.EditorName // Populate if you have logic to fetch editor name
            }).ToList();

            return Ok(feedbackDtos);
        }


        // ✅ FIXED REJECT BLOG
        [HttpPost("reject")]
        public async Task<IActionResult> RejectBlog([FromBody] RejectBlogRequest request)
        {
            // 1. Get the blog to fetch author_uid (REQUIRED - NOT NULL constraint)
            var blogResponse = await _supabase
                .From<BlogData>()
                .Where(b => b.Id == request.BlogId)
                .Single();

            var blog = blogResponse;
            if (blog == null)
                return NotFound("Blog not found");

            // 2. Update blog status
            await _supabase
                .From<BlogData>()
                .Where(b => b.Id == request.BlogId)
                .Set(b => b.Status!, "rejected")
                .Set(b => b.EditorUid!, request.EditorUid)
                .Set(b => b.ReviewedAt!, DateTime.UtcNow)
                .Update();

            // 3. Insert feedback WITH author_uid (fixes NOT NULL violation)
            await _supabase
                .From<EditorFeedback>()
                .Insert(new EditorFeedback
                {
                    BlogId = request.BlogId,
                    AuthorUid = blog.AuthorUid!,  // ✅ CRITICAL: From blog owner
                    EditorUid = request.EditorUid,
                    FeedbackType = "rejected",
                    Message = request.Comment,
                    CreatedAt = DateTime.UtcNow,
                    IsAuthorVisible = true,
                        EditorName = request.EditorName
                    // EditorName removed - not in EditorFeedback model
                });

            return Ok();
        }




        // ==============================
        // 6️⃣ POST: api/editor/update-content/{id}
        // ==============================
        [HttpPost("update-content/{id}")]
        public async Task<IActionResult> UpdateBlogContent(int id, [FromBody] RevisionModel revision)
        {
            // Insert new revision
            revision.CreatedAt = DateTime.UtcNow;
            var revisionData = new BlogRevisions
            {
                BlogId = revision.BlogId,
                VersionNo = revision.VersionNo,
                Content = revision.Content,
                EditorUid = revision.EditorUid,
                AuthorUid = revision.AuthorUid,
                IsCurrent = revision.IsCurrent,
                CreatedAt = DateTime.UtcNow,
                
            };

            await _supabase.From<BlogRevisions>().Insert(new List<BlogRevisions> { revisionData });

            // Update main blog content
            var result = await _supabase.From<BlogData>()
                                        .Where(b => b.Id == id)
                                        .Get();

            var blog = result.Models.FirstOrDefault();
            if (blog == null) return NotFound();

            blog.Content = revision.Content;
            blog.UpdatedAt = DateTime.UtcNow;

            await _supabase.From<BlogData>().Update(blog);
            return Ok(new { message = "Content updated successfully." });
        }

        // ==============================
        // 7️⃣ GET: api/editor/revisions/{blog_id}
        // ==============================
        [HttpGet("revisions/{blog_id}")]
        public async Task<IActionResult> GetRevisions(int blog_id)
        {
            var result = await _supabase.From<BlogRevisions>()
                                        .Where(r => r.BlogId == blog_id)
                                        .Order("created_at", Ordering.Ascending)
                                        .Get();

            return Ok(result.Models);
        }

        // ==============================
        // 8️⃣ POST: api/editor/revisions/restore/{version_id}
        // ==============================
        [HttpPost("revisions/restore/{version_id}")]
        public async Task<IActionResult> RestoreRevision(int version_id)
        {
            var response = await _supabase.From<BlogRevisions>()
                                          .Where(r => r.Id == version_id)
                                          .Get();

            var revision = response.Models.FirstOrDefault();
            if (revision == null) return NotFound();

            // Update main blog content
            var blog = await _supabase.From<BlogData>()
                                      .Where(b => b.Id == revision.BlogId)
                                      .Get();

            var target = blog.Models.FirstOrDefault();
            if (target == null) return NotFound();

            target.Content = revision.Content;
            target.UpdatedAt = DateTime.UtcNow;

            await _supabase.From<BlogData>().Update(target);

            return Ok(new { message = "Version restored successfully." });
        }

        // ==============================
        // 9️⃣ GET: api/editor/feedback/{blog_id}
        // ==============================
        [HttpGet("feedback/{blog_id}")]
        public async Task<IActionResult> GetFeedback(int blog_id)
        {
            var result = await _supabase.From<EditorFeedback>()
                                        .Where(f => f.BlogId == blog_id)
                                        .Order("created_at", Ordering.Ascending)
                                        .Get();

            return Ok(result.Models);
        }

        // ==============================
        // 🔟 GET: api/editor/analytics/{editor_uid}
        // ==============================
        [HttpGet("analytics/{editor_uid}")]
        public async Task<IActionResult> GetAnalytics(string editor_uid)
        {
            // Get blogs reviewed by this editor
            var result = await _supabase.From<BlogData>()
                                        .Where(b => b.EditorUid == editor_uid)
                                        .Get();

            var blogs = result.Models;

            var totalReviewed = blogs.Count;
            var totalApproved = blogs.Count(b => b.Status == "approved");
            var totalRejected = blogs.Count(b => b.Status == "rejected");
            var totalPending = blogs.Count(b => b.Status == "pending");

            // Calculate average review time (if ReviewedAt and CreatedAt exist)
            var reviewTimes = blogs
                .Where(b => b.ReviewedAt.HasValue)
                .Select(b => (b.ReviewedAt.Value - b.CreatedAt).TotalHours)
                .ToList();

            double avgReviewTime = reviewTimes.Count > 0 ? reviewTimes.Average() : 0;

            var analytics = new AnalyticsModel
            {
                TotalReviewed = totalReviewed,
                TotalApproved = totalApproved,
                TotalRejected = totalRejected,
                TotalPending = totalPending,
                AverageReviewTimeHours = Math.Round(avgReviewTime, 2)
            };

            return Ok(analytics);
        }
        [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateBlog(int id, [FromBody] BlogDetailModel model)
        {
            try
            {
                var response = await _supabase.From<BlogData>()
                                              .Where(b => b.Id == id)
                                              .Get();

                var blog = response.Models.FirstOrDefault();
                if (blog == null)
                    return NotFound(new { message = "Blog not found." });

                // --- Update basic fields (only if sent) ---
                if (!string.IsNullOrWhiteSpace(model.Title))
                    blog.Title = model.Title;

                if (!string.IsNullOrWhiteSpace(model.Slug))
                    blog.Slug = model.Slug;

                if (!string.IsNullOrWhiteSpace(model.MetaDescription))
                    blog.MetaDescription = model.MetaDescription;

                if (!string.IsNullOrWhiteSpace(model.Domain))
                    blog.Domain = model.Domain;

                if (model.Tags != null && model.Tags.Count > 0)
                    blog.Tags = model.Tags.ToList();

                if (!string.IsNullOrWhiteSpace(model.CoverImageUrl))
                    blog.CoverImageUrl = model.CoverImageUrl;

                if (!string.IsNullOrWhiteSpace(model.Content))
                    blog.Content = model.Content;

                // --- Update review/status info if applicable ---
                if (!string.IsNullOrWhiteSpace(model.Status))
                    blog.Status = model.Status;

                blog.EditorUid = model.EditorUid;
                blog.UpdatedAt = DateTime.UtcNow;

                await _supabase.From<BlogData>().Update(blog);

                return Ok(new { message = "Blog updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EditorController.UpdateBlog] Error: {ex.Message}");
                return StatusCode(500, new { message = "Error updating blog.", error = ex.Message });
            }
        }
        [HttpGet("dashboardstats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            // pending
            var pendingResult = await _supabase.From<BlogData>()
                .Where(b => b.Status == "pending")
                .Get();
            int pendingCount = pendingResult.Models.Count;

            // approved
            var approvedResult = await _supabase.From<BlogData>()
                .Where(b => b.Status == "approved")
                .Get();
            int approvedCount = approvedResult.Models.Count;

            // rejected
            var rejectedResult = await _supabase.From<BlogData>()
                .Where(b => b.Status == "rejected")
                .Get();
            int rejectedCount = rejectedResult.Models.Count;

            // revisions count (assuming you have table blog_revisions)
            var revisionsResult = await _supabase.From<BlogRevisions>()
                .Get();
            int revisionCount = revisionsResult.Models.Count;

            var stats = new DashboardStatsModel
            {
                PendingCount = pendingCount,
                ApprovedCount = approvedCount,
                RejectedCount = rejectedCount,
                RevisionCount = revisionCount,
            };

            return Ok(stats);
        }
        [HttpGet("{editorUid}")]
        public async Task<ActionResult<EditorProfileResponse>> GetEditorProfile(string editorUid)
        {
            if (string.IsNullOrWhiteSpace(editorUid))
                return BadRequest("Editor UID is required.");

            // parse first, outside expression
            if (!Guid.TryParse(editorUid, out var editorGuid))
                return BadRequest("Invalid editor UID.");

            // 1) Get BlogUser by UserId (auth UUID)
            var userResponse = await _supabase
                .From<BlogUser>()
                .Where(u => u.UserId == editorGuid)
                .Single();

            var blogUser = userResponse;   // <- use Model

            if (blogUser == null)
                return NotFound("Editor profile not found.");

            var userDto = new BlogUserDto
            {
                Id = blogUser.Id,
                UserId = blogUser.UserId,
                FullName = blogUser.FullName ?? "",
                Username = blogUser.Username,
                Email = blogUser.Email,
                AvatarUrl = blogUser.AvatarUrl,
                Bio = blogUser.Bio,
                Location = blogUser.Location,
                WebsiteUrl = blogUser.WebsiteUrl,
                Twitter = blogUser.Twitter,
                LinkedIn = blogUser.LinkedIn,
                Github = blogUser.Github,
                Instagram = blogUser.Instagram,
                Medium = blogUser.Medium,
                Roles = blogUser.Roles,
                IsActive = blogUser.IsActive,
                IsVerified = blogUser.IsVerified,
                PostCount = blogUser.PostCount,
                ViewCount = blogUser.ViewCount,
                EmailNotifications = blogUser.EmailNotifications,
                NewsletterSubscribed = blogUser.NewsletterSubscribed,
                Theme = blogUser.Theme,
                Language = blogUser.Language,
                CreatedAt = blogUser.CreatedAt,
                UpdatedAt = blogUser.UpdatedAt,
                LikeId = blogUser.LikeId,
                SavedId = blogUser.SavedId,
                Following = blogUser.Following,
                Follower = blogUser.Follower
            };

            // 2) Get blogs this editor has reviewed/approved (editor_uid = editorUid string)
            var blogsResponse = await _supabase
                .From<BlogData>()
                .Where(b => b.EditorUid == editorUid)
                .Get();

            var reviewedBlogsDto = blogsResponse.Models.Select(b => new BlogDto
            {
                Id = b.Id,
                Title = b.Title,
                Slug = b.Slug,
                Content = b.Content,
                CoverImageUrl = b.CoverImageUrl,
                AuthorName = b.AuthorName,
                AuthorUid = b.AuthorUid,
                Tags = b.Tags ?? new List<string>(),
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
                EditorUid = Guid.TryParse(b.EditorUid, out var g) ? g : null
            }).ToList();

            var result = new EditorProfileResponse
            {
                User = userDto,
                ReviewedBlogs = reviewedBlogsDto
            };

            return Ok(result);
        }

    }
}
