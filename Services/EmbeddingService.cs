//using System.Net.Http;
//using System.Text;
//using System.Text.Json;

//namespace BlogApp1.Server.Services
//{
//    public class EmbeddingService
//    {
//        private readonly HttpClient _http;

//        public EmbeddingService()
//        {
//            _http = new HttpClient();
//        }

//        public async Task<float[]> GetEmbedding(string text)
//        {
//            var payload = new
//            {
//                text = text
//            };

//            var json = JsonSerializer.Serialize(payload);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            var response = await _http.PostAsync(
//                "http://localhost:8000/embed",
//                content);

//            var result = await response.Content.ReadAsStringAsync();

//            var obj = JsonSerializer.Deserialize<EmbeddingResponse>(result);
//            return obj.embedding;
//        }
//    }

//    public class EmbeddingResponse
//    {
//        public float[] embedding { get; set; }
//    }
//}
