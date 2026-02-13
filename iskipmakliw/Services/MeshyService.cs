using iskipmakliw.Models.DTO;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace iskipmakliw.Services
{
    public interface IMeshyService
    {
        Task<MeshyApiResponse> CreateSingleImageTo3DTask(IFormFile imageFile);
        Task<MeshyApiResponse> CreateMultiImageTo3DTask(List<IFormFile> imageFiles);
        Task<MeshyApiResponse> GetTaskStatus(string taskId, bool isMulti);
        Task<byte[]> DownloadModel(string modelUrl);
    }

    public class MeshyService : IMeshyService
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://api.meshy.ai";

        public MeshyService(IConfiguration configuration)
        {
            var apiKey = configuration["Meshy:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Meshy API key is not configured. Add 'Meshy:ApiKey' to appsettings.json");

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromMinutes(2)  // longer timeout for large images
            };
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);
        }

        // ─── HELPERS ────────────────────────────────────────────────────────

        private static async Task<string> ToBase64DataUri(IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            var bytes = ms.ToArray();

            // Validate image size (Meshy recommends < 10MB)
            if (bytes.Length > 10 * 1024 * 1024)
                throw new Exception("Image too large. Please use images under 10MB.");

            var b64 = Convert.ToBase64String(bytes);
            return $"data:{file.ContentType};base64,{b64}";
        }

        private static MeshyApiResponse ParseCreatedTask(string json)
        {
            try
            {
                var doc = JsonSerializer.Deserialize<JsonElement>(json);

                // Try to get the result field - it might be named differently
                string taskId = null;

                if (doc.TryGetProperty("result", out var resultProp) && resultProp.ValueKind != JsonValueKind.Null)
                {
                    taskId = resultProp.GetString();
                }
                else if (doc.TryGetProperty("id", out var idProp) && idProp.ValueKind != JsonValueKind.Null)
                {
                    taskId = idProp.GetString();
                }
                else if (doc.TryGetProperty("task_id", out var taskIdProp) && taskIdProp.ValueKind != JsonValueKind.Null)
                {
                    taskId = taskIdProp.GetString();
                }

                if (string.IsNullOrEmpty(taskId))
                {
                    Console.WriteLine($"Failed to parse task ID from response: {json}");
                    throw new Exception($"Could not find task ID in API response. Response was: {json}");
                }

                return new MeshyApiResponse
                {
                    Id = taskId,
                    Status = "PENDING",
                    Progress = 0,
                    CreatedAt = DateTime.UtcNow
                };
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"JSON parsing error: {ex.Message}");
                Console.WriteLine($"Response was: {json}");
                throw new Exception($"Failed to parse API response: {ex.Message}");
            }
        }

        // ─── SINGLE IMAGE ───────────────────────────────────────────────────

        public async Task<MeshyApiResponse> CreateSingleImageTo3DTask(IFormFile imageFile)
        {
            var dataUri = await ToBase64DataUri(imageFile);

            // Use meshy-4 - it's the current stable model
            // DO NOT include enable_pbr, should_remesh, should_texture - they cause failures
            var body = new
            {
                image_url = dataUri,
                ai_model = "meshy-4"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            Console.WriteLine($"[Single] Sending request body: {JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true })}");

            var response = await _httpClient.PostAsync("/openapi/v1/image-to-3d", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Single] Status: {response.StatusCode}");
            Console.WriteLine($"[Single] Response: {jsonResponse}");

            if (!response.IsSuccessStatusCode)
            {
                // Try to parse error message from Meshy
                string errorMsg = "API request failed";
                try
                {
                    var errorDoc = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                    if (errorDoc.TryGetProperty("message", out var msg))
                        errorMsg = msg.GetString();
                }
                catch { /* use default message */ }

                throw new Exception($"Meshy API Error ({response.StatusCode}): {errorMsg}");
            }

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
                ai_model = "meshy-4"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            Console.WriteLine($"[Multi] Sending request body: {JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true })}");

            var response = await _httpClient.PostAsync("/openapi/v1/multi-image-to-3d", content);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"[Multi] Status: {response.StatusCode}");
            Console.WriteLine($"[Multi] Response: {jsonResponse}");

            if (!response.IsSuccessStatusCode)
            {
                string errorMsg = "API request failed";
                try
                {
                    var errorDoc = JsonSerializer.Deserialize<JsonElement>(jsonResponse);
                    if (errorDoc.TryGetProperty("message", out var msg))
                        errorMsg = msg.GetString();
                }
                catch { /* use default message */ }

                throw new Exception($"Meshy API Error ({response.StatusCode}): {errorMsg}");
            }

            return ParseCreatedTask(jsonResponse);
        }

        // ─── POLL STATUS ────────────────────────────────────────────────────

        public async Task<MeshyApiResponse> GetTaskStatus(string taskId, bool isMulti)
        {
            var endpoint = isMulti
                ? $"/openapi/v1/multi-image-to-3d/{taskId}"
                : $"/openapi/v1/image-to-3d/{taskId}";

            var response = await _httpClient.GetAsync(endpoint);
            var jsonResponse = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Status check failed ({response.StatusCode}): {jsonResponse}");

            var task = JsonSerializer.Deserialize<JsonElement>(jsonResponse);

            var result = new MeshyApiResponse
            {
                Id = taskId, // Use the taskId we passed in, safer than parsing
                Status = task.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : "UNKNOWN",
                Progress = task.TryGetProperty("progress", out var progressProp) ? progressProp.GetInt32() : 0
            };

            // Check for task errors
            if (task.TryGetProperty("task_error", out var taskError) && taskError.ValueKind != JsonValueKind.Null)
            {
                if (taskError.TryGetProperty("message", out var errMsg))
                {
                    var errorMessage = errMsg.GetString();
                    Console.WriteLine($"Task {taskId} error: {errorMessage}");

                    result.Status = "FAILED";
                    result.ThumbnailUrl = errorMessage;
                }
            }

            // Get model URL if succeeded
            if (result.Status == "SUCCEEDED" && task.TryGetProperty("model_urls", out var modelUrls) && modelUrls.ValueKind != JsonValueKind.Null)
            {
                if (modelUrls.TryGetProperty("glb", out var glbUrl) && glbUrl.ValueKind != JsonValueKind.Null)
                {
                    var url = glbUrl.GetString();
                    if (!string.IsNullOrEmpty(url))
                        result.ModelUrl = url;
                }
            }

            if (task.TryGetProperty("thumbnail_url", out var thumb) && thumb.ValueKind != JsonValueKind.Null)
            {
                var thumbUrl = thumb.GetString();
                if (!string.IsNullOrEmpty(thumbUrl) && result.Status != "FAILED")
                    result.ThumbnailUrl = thumbUrl;
            }

            return result;
        }

        // ─── DOWNLOAD ───────────────────────────────────────────────────────

        public async Task<byte[]> DownloadModel(string modelUrl)
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
            return await client.GetByteArrayAsync(modelUrl);
        }
    }
}