using Microsoft.AspNetCore.Mvc;

namespace BlogApp1.Server.Controllers
{
    public class CreateImageRequest
    {
        public string Name { get; set; }
        public IFormFile Image { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : Controller
    {
        [HttpPost]
        public async Task<IActionResult> UploadImage([FromForm] CreateImageRequest request,
             [FromServices] Supabase.Client client)
        {
            using var memoryStream = new MemoryStream();
            await request.Image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var bucket = client.Storage.From("blog-image"); // Your bucket name
            var fileName = $"{Guid.NewGuid()}_{request.Image.FileName}";
            await bucket.Upload(imageBytes, fileName);

            var publicUrl = bucket.GetPublicUrl(fileName);

            return Ok(new { Url = publicUrl });
        }
    }

}
