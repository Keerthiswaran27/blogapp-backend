using BlogApp1.Shared;
using Microsoft.AspNetCore.Mvc;
using Supabase;
using System;
using System.Threading.Tasks;

namespace YourApp.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsletterController : ControllerBase
    {
        private readonly Client _supabase;

        public NewsletterController(Client supabase)
        {
            _supabase = supabase;
        }

        // POST: api/newsletter/subscribe
        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] CreateNewsletterSubscriptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.EmailId) ||
                string.IsNullOrWhiteSpace(request.UserUuid))
            {
                return BadRequest("EmailId and UserUuid are required.");
            }

            var model = new NewsletterSubscription
            {
                EmailId = request.EmailId,
                UserUuid = request.UserUuid,
                CreatedAt = DateTime.UtcNow
            };

            // If user_uuid is UNIQUE in DB, duplicate insert will throw error;
            // you can catch and return 409 if needed.
            var result = await _supabase
                .From<NewsletterSubscription>()
                .Insert(model); // standard Supabase C# insert pattern[web:31][web:14]

            if (result.Models.Count == 0)
                return StatusCode(500, "Failed to create subscription.");

            return Ok(new
            {
                message = "Subscribed successfully",
                id = result.Models[0].Id
            });
        }

        // POST: api/newsletter/check
        [HttpPost("check")]
        public async Task<ActionResult<CheckNewsletterSubscriptionResponse>> Check(
            [FromBody] CheckNewsletterSubscriptionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.UserUuid))
            {
                return BadRequest("UserUuid is required.");
            }

            var response = await _supabase
                .From<NewsletterSubscription>()
                .Where(x => x.UserUuid == request.UserUuid)
                .Get(); // typical Supabase C# filter + Get()[web:12][web:14]

            var isPresent = response.Models.Count > 0;

            return Ok(new CheckNewsletterSubscriptionResponse
            {
                Status = isPresent ? "present" : "not_present"
            });
        }
    }
}
