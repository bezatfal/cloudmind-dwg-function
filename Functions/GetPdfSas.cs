using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace cloudmind_dwg_function.Functions
{
    public class GetPdfSas
    {
        private readonly BlobServiceClient _blobClient;
        private readonly string _outputContainer;

        public GetPdfSas(IConfiguration config)
        {
            _blobClient = new BlobServiceClient(config["AzureWebJobsStorage"]);
            _outputContainer = config["DWG_OUTPUT_CONTAINER"];
        }

        [Function("get-pdf-sas")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext ctx)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var filename = query["file"];

            var container = _blobClient.GetBlobContainerClient(_outputContainer);
            var blob = container.GetBlobClient(filename);

            var sas = blob.GenerateSasUri(BlobSasPermissions.Read, DateTimeOffset.UtcNow.AddHours(1));

            var res = req.CreateResponse();
            await res.WriteAsJsonAsync(new { url = sas.ToString() });
            return res;
        }
    }
}
