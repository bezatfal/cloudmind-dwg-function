using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace cloudmind_dwg_function.Functions
{
    public class GetUploadSas
    {
        private readonly BlobServiceClient _blobClient;
        private readonly string _inputContainer;

        public GetUploadSas(IConfiguration config)
        {
            _blobClient = new BlobServiceClient(config["AzureWebJobsStorage"]);
            _inputContainer = config["DWG_INPUT_CONTAINER"];
        }

        [Function("get-upload-sas")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext ctx)
        {
            var query = System.Web.HttpUtility.ParseQueryString(req.Url.Query);
            var filename = query["name"];

            var container = _blobClient.GetBlobContainerClient(_inputContainer);
            var blob = container.GetBlobClient(filename);

            var sas = blob.GenerateSasUri(BlobSasPermissions.Write, DateTimeOffset.UtcNow.AddMinutes(30));

            var res = req.CreateResponse();
            await res.WriteAsJsonAsync(new { uploadUrl = sas.ToString() });
            return res;
        }
    }
}
