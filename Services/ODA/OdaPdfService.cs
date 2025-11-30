using System.Diagnostics;

namespace cloudmind_dwg_function.Services.ODA
{
    public class OdaPdfService
    {
        private readonly string _odaPath;

        public OdaPdfService()
        {
            _odaPath = Environment.GetEnvironmentVariable("ODA_RUNTIME")
                ?? throw new Exception("ODA_RUNTIME not set");

            if (!Directory.Exists(_odaPath))
                throw new Exception($"ODA directory does not exist: {_odaPath}");
        }

        public async Task<string> ConvertToPdfAsync(string dwgPath)
        {
            // Ensure input file exists
            if (!File.Exists(dwgPath))
                throw new Exception($"Input DWG not found: {dwgPath}");

            // Output MUST be in /tmp for Linux Functions
            string pdfPath = Path.Combine("/tmp", $"{Path.GetFileName(dwgPath)}.pdf");

            // ODA executable path
            string exePath = Path.Combine(_odaPath, "OdPdfExportEx");

            if (!File.Exists(exePath))
                throw new Exception($"ODA converter not found: {exePath}");

            // Export flags: 1 = export all layouts
            string arguments = $"\"{dwgPath}\" \"{pdfPath}\" 1";

            var psi = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                WorkingDirectory = _odaPath,

                RedirectStandardError = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
            };

            var process = Process.Start(psi);
            if (process == null)
                throw new Exception("Failed to start ODA process.");

            string stdout = await process.StandardOutput.ReadToEndAsync();
            string stderr = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception(
                    $"ODA PDF conversion failed. Exit={process.ExitCode}\n" +
                    $"STDOUT:\n{stdout}\n" +
                    $"STDERR:\n{stderr}"
                );

            if (!File.Exists(pdfPath))
                throw new Exception("ODA finished but no PDF file was created.");

            return pdfPath;
        }
    }
}
