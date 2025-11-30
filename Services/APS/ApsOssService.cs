using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace cloudmind_dwg_function.Services.APS
{
    public class ApsOssService
    {
        private readonly HttpClient _http = new();
        private readonly string _bucket;

        public ApsOssService(IConfiguration config)
        {
            var clientId = config["APS_CLIENT_ID"].ToLower();
            _bucket = $"{clientId}-cloudmind-dwg";
        }

        public async Task<string> UploadDwgAsync(string token, Stream file, string name)
        {
            // 1) Create the bucket (new Storage v2)
            await EnsureBucketExistsAsync(token);

            // 2) Create an object registration
            var objectId = await RegisterObjectAsync(token, name, file.Length);

            // 3) Get the upload URL (v2)
            var uploadUrl = await GetUploadUrlAsync(token, objectId);

            // 4) Upload the file to that URL
            await UploadFileToUrlAsync(uploadUrl, file);

            // 5) Complete upload
            await CompleteUploadAsync(token, objectId);

            return objectId;
        }

        private async Task EnsureBucketExistsAsync(string token)
        {
            var url = $"https://developer.api.autodesk.com/data/v2/buckets/{_bucket}";

            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);

            if (resp.IsSuccessStatusCode)
                return;

            // Create bucket (storage v2)
            var body = new
            {
                bucketKey = _bucket,
                policyKey = "transient"
            };

            var create = new HttpRequestMessage(HttpMethod.Post, "https://developer.api.autodesk.com/data/v2/buckets");
            create.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            create.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var createResp = await _http.SendAsync(create);

            if (!createResp.IsSuccessStatusCode)
            {
                throw new Exception($"Bucket create failed: {await createResp.Content.ReadAsStringAsync()}");
            }
        }

        private async Task<string> RegisterObjectAsync(string token, string name, long size)
        {
            var safe = Uri.EscapeDataString(name);

            var url = $"https://developer.api.autodesk.com/data/v2/buckets/{_bucket}/objects";

            var body = new
            {
                fileName = safe,
                contentType = "application/octet-stream",
                contentLength = size
            };

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

            var resp = await _http.SendAsync(req);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Object registration failed: {json}");

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("objectId").GetString()!;
        }

        private async Task<string> GetUploadUrlAsync(string token, string objectId)
        {
            var url = $"https://developer.api.autodesk.com/data/v2/objects/{objectId}/upload-urls";

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Get upload URL failed: {json}");

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("urls")[0].GetString()!;
        }

        private async Task UploadFileToUrlAsync(string uploadUrl, Stream file)
        {
            var put = new HttpRequestMessage(HttpMethod.Put, uploadUrl);
            put.Content = new StreamContent(file);
            put.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var resp = await _http.SendAsync(put);
            if (!resp.IsSuccessStatusCode)
            {
                throw new Exception($"File upload failed: {resp.StatusCode} — {await resp.Content.ReadAsStringAsync()}");
            }
        }

        private async Task CompleteUploadAsync(string token, string objectId)
        {
            var url = $"https://developer.api.autodesk.com/data/v2/objects/{objectId}/upload-complete";

            var req = new HttpRequestMessage(HttpMethod.Post, url);
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var resp = await _http.SendAsync(req);
            var json = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new Exception($"Upload completion failed: {json}");
        }
    }
}
