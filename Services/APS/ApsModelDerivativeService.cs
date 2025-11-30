using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace cloudmind_dwg_function.Services.APS
{
    public class ApsModelDerivativeService
    {
        private readonly HttpClient _http;

        public ApsModelDerivativeService(IHttpClientFactory factory)
        {
            _http = factory.CreateClient();
        }

        public async Task<string> TranslateToPdfAsync(string token, string ossUrn)
        {
            string base64Urn = Convert.ToBase64String(Encoding.UTF8.GetBytes(ossUrn));

            var job = new
            {
                input = new { urn = base64Urn },
                output = new
                {
                    formats = new[]
                    {
                        new { type = "pdf", views = new[] { "2d" } }
                    }
                }
            };

            var req = new HttpRequestMessage(
                HttpMethod.Post,
                "https://developer.api.autodesk.com/modelderivative/v2/designdata/job");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            req.Content = new StringContent(JsonSerializer.Serialize(job));
            req.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            return base64Urn;
        }

        public async Task WaitForCompletionAsync(string token, string urn)
        {
            while (true)
            {
                var req = new HttpRequestMessage(
                    HttpMethod.Get,
                    $"https://developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/manifest");

                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var resp = await _http.SendAsync(req);
                var json = await resp.Content.ReadAsStringAsync();

                if (json.Contains("\"status\":\"success\""))
                    return;

                await Task.Delay(2000);
            }
        }

        public async Task<byte[]> DownloadPdfAsync(string token, string urn)
        {
            var req = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/manifest");

            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req);
            resp.EnsureSuccessStatusCode();

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);

            // Find PDF derivative
            var derivatives = doc.RootElement
                .GetProperty("derivatives")[0]
                .GetProperty("children")[0]
                .GetProperty("children");

            string pdfUrn = derivatives
                .EnumerateArray()
                .First(e => e.GetProperty("mime").GetString() == "application/pdf")
                .GetProperty("urn")
                .GetString()!;

            // Download PDF binary
            var pdfReq = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://developer.api.autodesk.com/modelderivative/v2/designdata/{urn}/manifest/{pdfUrn}");

            pdfReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var pdfResp = await _http.SendAsync(pdfReq);
            pdfResp.EnsureSuccessStatusCode();

            return await pdfResp.Content.ReadAsByteArrayAsync();
        }
    }
}
