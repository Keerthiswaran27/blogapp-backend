using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using Supabase.Gotrue; // <-- For User
using Supabase.Postgrest.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BlogApp1.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SiteController : ControllerBase
    {
        private readonly Supabase.Client _supabase; // Database client
        private readonly AdminClient _adminClient; // Admin client for auth.users

        public SiteController(IConfiguration config)
        {
            var supabaseUrl = config["Supabase:Url"];
            var supabaseServiceKey = config["Supabase:Key"]; // MUST be service role key

            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,
                AutoConnectRealtime = true
            };

            _supabase = new Supabase.Client(supabaseUrl, supabaseServiceKey, options);

            // Initialize AdminClient for user operations
            _adminClient = new AdminClient(supabaseServiceKey);
        }

        [HttpGet("overallstats")]
        public async Task<IActionResult> GetStatsAsync()
        {
            // ---------- Blog Stats (from public schema) ----------
            var blogsResult = await _supabase.From<BlogData>().Get();
            var blogs = blogsResult.Models;

            int pendingBlogs = blogs.Count(b => b.Status == "pending");

            // ---------- User Stats (from auth.users via AdminClient) ----------
            var users = await _adminClient.ListUsers();
            int totalUsers = users.Users.Count;
            int activeUsers = users.Users.Count(u => u.LastSignInAt != null &&
                                               u.LastSignInAt > DateTime.UtcNow.AddDays(-30));

            // ---------- Final Output ----------
            var output = new Stats
            {
                TotalUsers = totalUsers,
                ActiveUsers = activeUsers,
                TotalBlogs = blogs.Count,
                PendingBlogs = pendingBlogs
            };

            return Ok(output);
        }
    }
}
