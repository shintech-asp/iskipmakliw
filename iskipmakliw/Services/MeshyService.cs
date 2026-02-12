using iskipmakliw.Models.DTO;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace iskipmakliw.Services
{
    public class MeshyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private const string BaseUrl = "https://api.meshy.ai";

        public MeshyService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _apiKey = configuration["Meshy:ApiKey"];

            if (string.IsNullOrEmpty(_apiKey))
                throw new ArgumentException("Meshy API key is not configured. Add 'Meshy:ApiKey' to appsettings.json");

            _httpClient.BaseAddress = new Uri(BaseUrl);
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
        }

        // ─── HELPERS ────────────────────────────────────────────────────────

        private static async Task<string> ToBase64DataUri(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var b64 = Convert.ToBase64String(ms.ToArray());
            return $"data:{file.ContentType};base64,{b64}";
        }

        private static MeshyApiResponse ParseCreatedTask(string json)
        {
            var doc = JsonSerializer.Deserialize<JsonElement>(json);
            return new MeshyApiResponse
            {
                Id = doc.GetProperty("result").GetString(),
                Status = "PENDING",
                Progress = 0,
                CreatedAt = DateTime.UtcNow
            };
        }

        // ─── SINGLE IMAGE ───────────────────────────────────────────────────

        public async Task<MeshyApiResponse> CreateSingleImageTo3DTask(IFormFile imageFile)
        {
            var dataUri = await ToBase64DataUri(imageFile);

            var body = new
            {
                image_url = dataUri,
                ai_model = "meshy-4",
                enable_pbr = true,
                should_remesh = true,
                should_texture = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/openapi/v1/image-to-3d", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Single] Status: {response.StatusCode} | Body: {jsonResponse}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error ({response.StatusCode}): {jsonResponse}");

            return ParseCreatedTask(jsonResponse);
        }

        // ─── MULTI IMAGE ────────────────────────────────────────────────────

        public async Task<MeshyApiResponse> CreateMultiImageTo3DTask(List<IFormFile> imageFiles)
        {
            var urls = new List<string>();
            foreach (var file in imageFiles)
                urls.Add(await ToBase64DataUri(file));

            var body = new
            {
                image_urls = urls,
                ai_model = "meshy-4",
                enable_pbr = true,
                should_remesh = true,
                should_texture = true
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/openapi/v1/multi-image-to-3d", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Multi] Status: {response.StatusCode} | Body: {jsonResponse}");

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error ({response.StatusCode}): {jsonResponse}");

            return ParseCreatedTask(jsonResponse);
        }

        // ─── POLL STATUS (works for both task types) ────────────────────────

        public async Task<MeshyApiResponse> GetTaskStatus(string taskId, bool isMulti)
        {
            var endpoint = isMulti
                ? $"/openapi/v1/multi-image-to-3d/{taskId}"
                : $"/openapi/v1/image-to-3d/{taskId}";

            var response = await _httpClient.GetAsync(endpoint);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"API Error ({response.StatusCode}): {jsonResponse}");

            var task = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            var result = new MeshyApiResponse
            {
                Id = task.GetProperty("id").GetString(),
                Status = task.GetProperty("status").GetString(),
                Progress = task.GetProperty("progress").GetInt32()
            };

            if (result.Status == "SUCCEEDED" && task.TryGetProperty("model_urls", out var modelUrls))
            {
                if (modelUrls.TryGetProperty("glb", out var glbUrl))
                    result.ModelUrl = glbUrl.GetString();
            }

            if (task.TryGetProperty("thumbnail_url", out var thumb))
                result.ThumbnailUrl = thumb.GetString();

            return result;
        }

        // ─── DOWNLOAD ───────────────────────────────────────────────────────

        public async Task<byte[]> DownloadModel(string modelUrl)
        {
            using var client = new HttpClient();
            return await client.GetByteArrayAsync(modelUrl);
        }
    }
}
