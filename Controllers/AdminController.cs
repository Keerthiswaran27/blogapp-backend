using Microsoft.AspNetCore.Mvc;
using BlogApp1.Shared;
using Supabase;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using static Supabase.Postgrest.Constants;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly Client _supabase;

        public AdminController(IConfiguration config)
        {
            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:Key"];

            _supabase = new Client(supabaseUrl, supabaseKey);
            _supabase.InitializeAsync().Wait(); // Initialize once
        }

        // ✅ GET: api/admin
        [HttpGet]
        public async Task<ActionResult<List<BlogUserDto>>> GetAllUsers()
        {
            var response = await _supabase
                .From<BlogUser>()
                .Get();

            var users = response.Models.Select(u => new BlogUserDto
            {
                Id = u.Id,
                UserId = u.UserId,
                FullName = u.FullName,
                Username = u.Username,
                Email = u.Email,
                AvatarUrl = u.AvatarUrl,
                Roles = u.Roles,
                IsActive = u.IsActive,
                IsVerified = u.IsVerified,
                Follower = u.Follower,
                Following = u.Following,
                PostCount = u.PostCount,
                ViewCount = u.ViewCount,
                CreatedAt = u.CreatedAt,
                UpdatedAt = u.UpdatedAt
            }).ToList();

            return Ok(users);
        }

        // ✅ GET: api/admin/getuser/{id}
        [HttpGet("getuser/{id}")]
        public async Task<ActionResult<BlogUserDto>> GetUserById(Guid id)
        {
            var user = await _supabase
                .From<BlogUser>()
                .Where(b => b.UserId == id)
                .Single();

            if (user == null) return NotFound();

        

            var dto = new BlogUserDto
            {
                Id = user.Id,
                UserId = user.UserId,
                FullName = user.FullName,
                Username = user.Username,
                Email = user.Email,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                Location = user.Location,
                WebsiteUrl = user.WebsiteUrl,
                Twitter = user.Twitter,
                LinkedIn = user.LinkedIn,
                Github = user.Github,
                Instagram = user.Instagram,
                Medium = user.Medium,
                Roles = user.Roles,
                IsActive = user.IsActive,
                IsVerified = user.IsVerified,
                PostCount = user.PostCount,
                ViewCount = user.ViewCount,
                EmailNotifications = user.EmailNotifications,
                NewsletterSubscribed = user.NewsletterSubscribed,
                Theme = user.Theme,
                Language = user.Language,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt,
                LikeId = user.LikeId,
                SavedId = user.SavedId,
                Following = user.Following,
                Follower = user.Follower
            };

            return Ok(dto);
        }

        // ✅ PUT: api/admin/updateuser/{id}
        [HttpPut("updateuser")]
        public async Task<IActionResult> UpdateUser([FromBody] BlogUserDto updatedUser)
        {
            //if (userId != updatedUser.Id)
            //    return BadRequest("User ID mismatch");

            // Fetch the existing user
            var existingUserResponse = await _supabase
                .From<BlogUser>()
                .Where(b => b.UserId == updatedUser.UserId)
                .Single();

            var existingUser = existingUserResponse;
            if (existingUser == null)
                return NotFound();

            // Update fields
            existingUser.FullName = updatedUser.FullName;
            existingUser.Username = updatedUser.Username;
            existingUser.Email = updatedUser.Email;
            existingUser.AvatarUrl = updatedUser.AvatarUrl;
            existingUser.Bio = updatedUser.Bio;
            existingUser.Location = updatedUser.Location;
            existingUser.WebsiteUrl = updatedUser.WebsiteUrl;
            existingUser.Twitter = updatedUser.Twitter;
            existingUser.LinkedIn = updatedUser.LinkedIn;
            existingUser.Github = updatedUser.Github;
            existingUser.Instagram = updatedUser.Instagram;
            existingUser.Medium = updatedUser.Medium;
            existingUser.Roles = updatedUser.Roles ?? existingUser.Roles;
            existingUser.IsActive = updatedUser.IsActive;
            existingUser.IsVerified = updatedUser.IsVerified;
            existingUser.EmailNotifications = updatedUser.EmailNotifications ?? existingUser.EmailNotifications;
            existingUser.NewsletterSubscribed = updatedUser.NewsletterSubscribed;
            existingUser.Theme = updatedUser.Theme;
            existingUser.Language = updatedUser.Language ?? existingUser.Language;
            existingUser.CreatedAt = updatedUser.CreatedAt;
            existingUser.UpdatedAt = DateTime.UtcNow;

            // Save changes
            await _supabase.From<BlogUser>().Upsert(existingUser);

            return NoContent();
        }


        // ✅ DELETE: api/admin/deleteuser/{id}
        [HttpDelete("deleteuser/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var response = await _supabase
            .From<BlogUser>()
            .Where(b => b.UserId==id)   // eq = equals
            .Single();


            var user = response;
            if (user == null)
                return NotFound();

            // Hard delete (if you want soft delete, just set IsActive = false)
            await _supabase.From<BlogUser>().Delete(user);

            return NoContent();
        }
        // ✅ POST: api/admin/getfollowers
        [HttpGet("getfollowers")]
        public async Task<ActionResult<List<string>>> GetFollowers([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest("Follower list is empty.");

            var followerIds = ids.Split(',').Select(Guid.Parse).Cast<object>().ToList();

            try
            {
                var result = await _supabase
                    .From<BlogUser>()
                    .Select("full_name")
                    .Filter("user_id", Operator.In, followerIds)
                    .Get();

                if (!result.Models.Any())
                    return NotFound("No users found for the provided IDs.");

                var fullNames = result.Models
                                      .Select(u => u.FullName)
                                      .Where(name => !string.IsNullOrEmpty(name))
                                      .ToList();

                return Ok(fullNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


        // ✅ POST: api/admin/getfollowing
        [HttpGet("getfollowing")]
        public async Task<ActionResult<List<string>>> GetFollowing([FromQuery] string ids)
        {
            if (string.IsNullOrEmpty(ids))
                return BadRequest("Following list is empty.");

            var followingIds = ids.Split(',').Select(Guid.Parse).Cast<object>().ToList();

            try
            {
                var result = await _supabase
                    .From<BlogUser>()
                    .Select("full_name")
                    .Filter("user_id", Operator.In, followingIds)
                    .Get();

                if (!result.Models.Any())
                    return NotFound("No users found for the provided IDs.");

                var fullNames = result.Models
                                      .Select(u => u.FullName)
                                      .Where(name => !string.IsNullOrEmpty(name))
                                      .ToList();

                return Ok(fullNames);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("blogid/{Id}")]
        public async Task<IActionResult> GetBlogById(int Id)
        {
            var result = await _supabase
                .From<BlogData>()
                .Where(p => p.Id==Id)
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
                UpdatedAt = result.UpdatedAt,
                MetaDescription = result.MetaDescription,
                PublishedAt = result.PublishedAt,
                ReadingTime = result.ReadingTime,
                Status = result.Status
            };

            return Ok(dto);
        }
        [HttpPut("update-blog")]
        public async Task<IActionResult> UpdateBlog([FromBody] BlogDto dto)
        {
            if (dto == null || dto.Id == 0)
                return BadRequest("Invalid blog data.");

            try
            {
                // 🔍 Fetch existing blog
                var blog = await _supabase
                    .From<BlogData>()
                    .Where(b => b.Id == dto.Id)
                    .Single();

                if (blog == null)
                    return NotFound("Blog not found.");

                // ✏️ Update fields
                blog.Title = dto.Title;
                blog.Slug = dto.Slug;
                blog.Content = dto.Content;
                blog.CoverImageUrl = dto.CoverImageUrl;
                blog.AuthorName = dto.AuthorName;
                blog.AuthorUid = dto.AuthorUid;
                blog.Tags = dto.Tags ?? new();
                blog.Domain = dto.Domain;
                blog.ViewCount = dto.ViewCount;
                blog.LikesCount = dto.LikesCount;
                blog.Status = dto.Status;
                blog.MetaDescription = dto.MetaDescription;
                blog.UpdatedAt = DateTime.UtcNow;
                blog.PublishedAt = dto.PublishedAt;
                blog.ReadingTime = dto.ReadingTime;

                // 💾 Update Supabase record
                await _supabase
                    .From<BlogData>()
                    .Where(b => b.Id == dto.Id)
                    .Update(blog);

                // ✅ Return updated DTO directly
                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating blog: {ex.Message}");
            }
        }
        [HttpGet("getallcomments")]
        public async Task<IActionResult> GetAllComments()
        {
            try
            {
                var comments = await _supabase
                    .From<BlogComment>()
                    .Order("created_at", Ordering.Descending)
                    .Get();

                // Map to DTO to avoid serializing attributes
                var dto = comments.Models.Select(c => new CommentDto
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
                }).ToList();

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching comments: {ex.Message}");
            }
        }


        // ✅ 2. Get single comment by CommentUid
        [HttpGet("getcomment/uid/{commentUid}")]
        public async Task<IActionResult> GetCommentById(Guid commentUid)
        {
            try
            {
                var comment = await _supabase
                    .From<BlogComment>()
                    .Where(c => c.CommentUid == commentUid)
                    .Single();

                if (comment == null)
                    return NotFound("Comment not found");

                // Map to DTO to avoid serialization of attributes
                var dto = new CommentDto
                {
                    CommentUid = comment.CommentUid,
                    BlogId = comment.BlogId,
                    CommentUserId = comment.CommentUserId,
                    ParentCommentUid = comment.ParentCommentUid,
                    IsParent = comment.IsParent,
                    Content = comment.Content,
                    LikesCount = comment.LikesCount,
                    CreatedAt = comment.CreatedAt,
                    UpdatedAt = comment.UpdatedAt,
                    AuthorName = comment.AuthorName
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error fetching comment: {ex.Message}");
            }
        }


        // ✅ 3. Update comment by CommentUid
        [HttpPut("updatecomment")]
        public async Task<IActionResult> UpdateComment([FromBody] CommentDto updatedComment)
        {
            if (updatedComment == null || updatedComment.CommentUid == Guid.Empty)
                return BadRequest("Invalid comment data");

            try
            {
                // Fetch existing comment
                var comment = await _supabase
                    .From<BlogComment>()
                    .Where(c => c.CommentUid == updatedComment.CommentUid)
                    .Single();

                if (comment == null)
                    return NotFound("Comment not found");

                // Update the allowed fields
                comment.BlogId = updatedComment.BlogId;
                comment.CommentUserId = updatedComment.CommentUserId;
                comment.ParentCommentUid = updatedComment.ParentCommentUid;
                comment.IsParent = updatedComment.IsParent;
                comment.Content = updatedComment.Content;
                comment.LikesCount = updatedComment.LikesCount;
                comment.CreatedAt = updatedComment.CreatedAt;
                comment.UpdatedAt = DateTime.UtcNow; // or use updatedComment.UpdatedAt if you want
                comment.AuthorName = updatedComment.AuthorName;

                var result = await _supabase
                    .From<BlogComment>()
                    .Update(comment);

                // Map to DTO before returning
                var updatedDto = new CommentDto
                {
                    CommentUid = result.Models.FirstOrDefault()?.CommentUid ?? Guid.Empty,
                    BlogId = result.Models.FirstOrDefault()?.BlogId ?? 0,
                    CommentUserId = result.Models.FirstOrDefault()?.CommentUserId ?? Guid.Empty,
                    ParentCommentUid = result.Models.FirstOrDefault()?.ParentCommentUid,
                    IsParent = result.Models.FirstOrDefault()?.IsParent ?? false,
                    Content = result.Models.FirstOrDefault()?.Content ?? "",
                    LikesCount = result.Models.FirstOrDefault()?.LikesCount ?? 0,
                    CreatedAt = result.Models.FirstOrDefault()?.CreatedAt ?? DateTime.UtcNow,
                    UpdatedAt = result.Models.FirstOrDefault()?.UpdatedAt,
                    AuthorName = result.Models.FirstOrDefault()?.AuthorName
                };

                return Ok(updatedDto);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error updating comment: {ex.Message}");
            }
        }
        [HttpDelete("delete-comment/{commentUid}")]
        public async Task<IActionResult> DeleteComment(Guid commentUid)
        {
            if (commentUid == Guid.Empty)
                return BadRequest("Invalid Comment UID.");

            try
            {
                // 🔍 Fetch the comment
                var comment = await _supabase
                    .From<BlogComment>()
                    .Where(c => c.CommentUid == commentUid)
                    .Single();

                if (comment == null)
                    return NotFound("Comment not found.");

                // 🧹 Delete the comment
                await _supabase
                    .From<BlogComment>()
                    .Where(c => c.CommentUid == commentUid)
                    .Delete();

                return Ok(new { message = "Comment deleted successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error deleting comment: {ex.Message}");
                return StatusCode(500, "An error occurred while deleting the comment.");
            }
        }
        [HttpGet("replies/{parentUid}")]
        public async Task<IActionResult> GetReplies(Guid parentUid)
        {
            try
            {
                // 🔍 Fetch all comments where ParentCommentUid matches
                var response = await _supabase
                    .From<BlogComment>()
                    .Where(c => c.ParentCommentUid == parentUid)
                    .Order(x => x.CreatedAt, Supabase.Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var replies = response.Models.Select(c => new CommentDto
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
                }).ToList();

                if (replies.Count == 0)
                    return NotFound(new { Message = "No replies found for this comment." });

                return Ok(replies);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Error fetching replies.", Error = ex.Message });
            }
        }

    }
}
