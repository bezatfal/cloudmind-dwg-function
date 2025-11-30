using cloudmind_dwg_function.Services.ODA;
using cloudmind_dwg_function;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var odaPath = Path.Combine(AppContext.BaseDirectory, "oda");

// 🔧 Must be set BEFORE host.Build()
Environment.SetEnvironmentVariable("ODA_RUNTIME", odaPath);

// 🔧 Fix LD_LIBRARY_PATH for Linux shared libs
var existingLd = Environment.GetEnvironmentVariable("LD_LIBRARY_PATH");
if (string.IsNullOrEmpty(existingLd))
{
    Environment.SetEnvironmentVariable("LD_LIBRARY_PATH", odaPath);
}
else
{
    Environment.SetEnvironmentVariable(
        "LD_LIBRARY_PATH",
        $"{existingLd}:{odaPath}"
    );
}

Console.WriteLine("==== ODA INIT ====");
Console.WriteLine($"ODA_RUNTIME = {Environment.GetEnvironmentVariable("ODA_RUNTIME")}");
Console.WriteLine($"LD_LIBRARY_PATH = {Environment.GetEnvironmentVariable("LD_LIBRARY_PATH")}");
Console.WriteLine("====================");

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // ODA PDF engine
        services.AddSingleton<OdaPdfService>();

        // Main processor
        services.AddSingleton<DwgProcessor>();
    })
    .Build();

host.Run();
