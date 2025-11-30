using System.Linq;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

namespace cloudmind_dwg_function.Functions
{
    public class ListPdfs
    {
        private readonly BlobServiceClient _blobClient;
        private readonly string _outputContainer;

        public ListPdfs(IConfiguration config)
        {
            _blobClient = new BlobServiceClient(config["AzureWebJobsStorage"]);
            _outputContainer = config["DWG_OUTPUT_CONTAINER"];
        }

        [Function("list-pdfs")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
            FunctionContext ctx)
        {
            var log = ctx.GetLogger("ListPdfs");

            var container = _blobClient.GetBlobContainerClient(_outputContainer);
            var blobs = container.GetBlobsAsync();

            var names = new List<string>();

            await foreach (var blob in blobs)
            {
                if (blob.Name.EndsWith(".pdf"))
                    names.Add(blob.Name);
            }

            var res = req.CreateResponse();
            res.Headers.Add("Content-Type", "application/json");
            await res.WriteStringAsync(JsonSerializer.Serialize(names));

            return res;
        }
    }
}
