using BlogApp1.Shared;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Gotrue;
using Supabase.Interfaces;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly Supabase.Client _supabase;

        public AuthController(IConfiguration config)
        {
            var options = new SupabaseOptions { AutoConnectRealtime = false };

            var supabaseUrl = config["Supabase:Url"];
            var supabaseKey = config["Supabase:Key"];

            _supabase = new Supabase.Client(supabaseUrl, supabaseKey);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupModel model)
        {
            try
            {
                var roleString = model.Role != null && model.Role.Any()
                    ? string.Join(",", model.Role)
                    : string.Empty;

                var response = await _supabase.Auth.SignUp(
                    model.Email,
                    model.Password,
                    new SignUpOptions
                    {
                        Data = new Dictionary<string, object>
                        {
                    { "name", model.Name },
                    { "role", roleString }
                        }
                    });

                if (response.User == null)
                {
                    return StatusCode(500, "Signup failed: user was not created.");
                }

                var userUuid = Guid.Parse(response.User.Id);

                // Only return the auth user id here
                return Ok(new SignupResponseDto
                {
                    UserId = userUuid
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Signup failed: {ex.Message}");
            }
        }
        


        [HttpPost("signin")]
        public async Task<IActionResult> Signin([FromBody] SignInModel model)
        {

            try
            {
                var session = await _supabase.Auth.SignInWithPassword(model.Email, model.Password);

                if (session?.User != null)
                {
                    var user_details = new
                    {
                        AccessToken = session.AccessToken,
                        Uid = session.User.Id
                    };
                    return Ok(user_details);
                }
                else
                {
                    return BadRequest("not connected");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Signin failed: {ex.Message}");
            }
        }
        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo([FromQuery] Guid uid)
        {
            try
            {
                var response = await _supabase
                    .From<BlogUser>()
                    .Where(b => b.UserId == uid)
                    .Get();

                var user = response.Models.FirstOrDefault();

                if (user == null)
                    return NotFound("User record not found");

                // Return the user info
                return Ok(new
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    Role = user.Roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving user info: {ex.Message}");
            }
        }
        [HttpPost("User-Preference")]
        public async Task<IActionResult> UpdateUserPreference([FromBody] UpdateUserPreferenceRequest request)
        {
            if (request == null || request.UserId == Guid.Empty)
                return BadRequest("Invalid request.");

            // Get the user row by user_id (or use Id if you prefer)
            var existing = await _supabase
                .From<BlogUser>()
                .Where(u => u.UserId == request.UserId)
                .Single();

            if (existing == null)
                return NotFound("User not found.");

            // Update the UserPreference array
            existing.UserPreference = request.Preferences?.ToArray() ?? Array.Empty<string>();
            existing.UpdatedAt = DateTime.UtcNow;

            // Persist to Supabase
            var updated = await existing.Update<BlogUser>();
            var Results = updated?.Model?.UserPreference?.ToList();
            return Ok(Results);
        }


        [HttpPost("create-profile")]
        public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request)
        {
            if (request.UserId == Guid.Empty)
                return BadRequest("Invalid user id.");

            try
            {
                var profile = new BlogUser
                {
                    UserId = request.UserId,
                    FullName = request.Name,
                    Email = request.Email,
                    Roles = request.Role?.ToArray(),
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _supabase.From<BlogUser>().Insert(profile);

                if (result.Models == null || !result.Models.Any())
                    return StatusCode(500, "Failed to insert user profile.");

                var inserted = result.Models.First();

                return Ok(new
                {
                    id = inserted.Id,
                    userId = inserted.UserId,
                    fullName = inserted.FullName,
                    email = inserted.Email,
                    roles = inserted.Roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Profile creation failed: {ex.Message}");
            }
        }

        [HttpGet("user-stats/{userId:guid}")]
        public async Task<ActionResult<UserStatsDto>> GetUserStats(Guid userId)
        {
            // 1. Load BlogUser by user_id
            var userResponse = await _supabase
                .From<BlogUser>()
                .Where(u => u.UserId== userId)
                .Single();

            var blogUser = userResponse;

            if (blogUser == null)
                return NotFound();

            // 2. Count comments where CommentUserId == userId
            var commentsResponse = await _supabase
                .From<BlogComment>()
                .Where(c => c.CommentUserId==userId)
                .Get();

            var comments = commentsResponse.Models;

            var dto = new UserStatsDto
            {
                UserId = userId,
                LikedCount = blogUser.LikeId?.Length ?? 0,
                SavedCount = blogUser.SavedId?.Length ?? 0,
                ReadHistoryCount = blogUser.ReadHistory?.Length ?? 0,
                CommentCount = comments.Count
            };

            return Ok(dto);
        }
        [HttpGet("user-profile-info/{userId:guid}")]
        public async Task<ActionResult<BlogUserDto>> GetByUserId(Guid userId)
        {
            var response = await _supabase
                .From<BlogUser>()
                .Where(u => u.UserId==userId)
                .Single();

            var user = response;

            if (user == null)
                return NotFound();

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
                Follower = user.Follower,
                ReadHistory = user.ReadHistory,
                UserPreference = user.UserPreference
            };

            return Ok(dto);
        }
        [HttpPut("editprofile")]
        public async Task<ActionResult<BlogUserDto>> UpdateProfile([FromBody] BlogUserDto dto)
        {
            if (dto == null || dto.UserId == Guid.Empty)
                return BadRequest("Invalid user data.");

            // Load existing BlogUser by UserId
            var response = await _supabase
                .From<BlogUser>()
                .Where(u => u.UserId== dto.UserId)
                .Single();

            var existing = response;

            if (existing == null)
                return NotFound();

            // Map editable fields from DTO to model
            existing.FullName = dto.FullName;
            existing.Username = dto.Username;
            existing.Email = dto.Email;
            existing.AvatarUrl = dto.AvatarUrl;
            existing.Bio = dto.Bio;
            existing.Location = dto.Location;
            existing.WebsiteUrl = dto.WebsiteUrl;
            existing.Twitter = dto.Twitter;
            existing.LinkedIn = dto.LinkedIn;
            existing.Github = dto.Github;
            existing.Instagram = dto.Instagram;
            existing.Medium = dto.Medium;

            existing.Roles = dto.Roles;
            existing.IsActive = dto.IsActive;
            existing.IsVerified = dto.IsVerified;

            existing.PostCount = dto.PostCount;
            existing.ViewCount = dto.ViewCount;

            existing.EmailNotifications = dto.EmailNotifications;
            existing.NewsletterSubscribed = dto.NewsletterSubscribed;
            existing.Theme = dto.Theme;

            existing.Language = dto.Language;
            existing.LikeId = dto.LikeId;
            existing.SavedId = dto.SavedId;
            existing.Following = dto.Following;
            existing.Follower = dto.Follower;
            existing.ReadHistory = dto.ReadHistory;
            existing.UserPreference = dto.UserPreference;

            existing.UpdatedAt = DateTime.UtcNow;

            // Persist changes
            await existing.Update<BlogUser>();

            // Optionally return the updated DTO
            return Ok(dto);
        }

    }
}
