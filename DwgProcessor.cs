using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using cloudmind_dwg_function.Services.ODA;

namespace cloudmind_dwg_function
{
    public class DwgProcessor
    {
        private readonly BlobServiceClient _blobClient;
        private readonly OdaPdfService _oda;
        private readonly string _outputContainer;

        public DwgProcessor(IConfiguration config, OdaPdfService oda)
        {
            _oda = oda;

            _outputContainer = config["DWG_OUTPUT_CONTAINER"]
                ?? throw new Exception("DWG_OUTPUT_CONTAINER missing");

            string conn = config["AzureWebJobsStorage"]
                ?? throw new Exception("AzureWebJobsStorage missing");

            _blobClient = new BlobServiceClient(conn);
        }

        [Function("DwgProcessor")]
        public async Task RunAsync(
            [BlobTrigger("%DWG_INPUT_CONTAINER%/{name}")] Stream inputBlob,
            string name,
            FunctionContext context)
        {
            var log = context.GetLogger("DWG");
            log.LogInformation($"➡ Triggered DWG Processor for: {name}");

            // -----------------------------------
            // 1️⃣ Save DWG to /tmp
            // -----------------------------------
            string safeName = name.Replace("/", "_");
            string tmpDwgPath = Path.Combine("/tmp", safeName);

            try
            {
                await using var fs = File.Create(tmpDwgPath);
                await inputBlob.CopyToAsync(fs);

                log.LogInformation($"✔ DWG saved to: {tmpDwgPath}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, $"❌ Failed writing DWG temp file: {tmpDwgPath}");
                throw;
            }

            // -----------------------------------
            // 2️⃣ Convert DWG → PDF
            // -----------------------------------
            string pdfPath;

            try
            {
                pdfPath = await _oda.ConvertToPdfAsync(tmpDwgPath);
                log.LogInformation($"✔ PDF created: {pdfPath}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "❌ ODA conversion failed");
                throw;
            }

            // Ensure file exists
            if (!File.Exists(pdfPath))
            {
                log.LogError($"❌ Expected PDF not found: {pdfPath}");
                throw new Exception($"PDF not created: {pdfPath}");
            }

            // -----------------------------------
            // 3️⃣ Upload PDF to blob storage
            // -----------------------------------
            try
            {
                var container = _blobClient.GetBlobContainerClient(_outputContainer);
                await container.CreateIfNotExistsAsync();

                // Actual file name on disk
                string pdfBlobName = Path.GetFileName(pdfPath);

                await using var pdfStream = File.OpenRead(pdfPath);
                await container.GetBlobClient(pdfBlobName)
                    .UploadAsync(pdfStream, overwrite: true);

                log.LogInformation($"🎉 Uploaded PDF: {_outputContainer}/{pdfBlobName}");
            }
            catch (Exception ex)
            {
                log.LogError(ex, "❌ Failed uploading PDF");
                throw;
            }

            // -----------------------------------
            // 4️⃣ Cleanup
            // -----------------------------------
            try
            {
                if (File.Exists(tmpDwgPath))
                    File.Delete(tmpDwgPath);

                if (File.Exists(pdfPath))
                    File.Delete(pdfPath);

                log.LogInformation("🧹 Cleanup complete");
            }
            catch
            {
                // Best effort cleanup
            }
        }
    }
}
