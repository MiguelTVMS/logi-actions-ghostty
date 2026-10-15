using System.Diagnostics;
using System.Reflection;
using System.Text.Json;

try
{
    using var manifest = JsonDocument.Parse(File.ReadAllText(".config/dotnet-tools.json"));
    var pinned = manifest.RootElement.GetProperty("tools").GetProperty("logiplugintool").GetProperty("version").GetString();
    using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    using var versions = JsonDocument.Parse(await client.GetStringAsync("https://api.nuget.org/v3-flatcontainer/logiplugintool/index.json"));
    var latest = versions.RootElement.GetProperty("versions").EnumerateArray()
        .Select(value => value.GetString()!).Where(value => !value.Contains('-'))
        .OrderBy(value => Version.Parse(value)).Last();
    Console.WriteLine($"LogiPluginTool: pinned {pinned}, latest {latest}");
    var apiDir = Environment.GetEnvironmentVariable("PluginApiDir")
        ?? "/Applications/Utilities/LogiPluginService.app/Contents/MonoBundle";
    var apiPath = Path.Combine(apiDir, "PluginApi.dll");
    Console.WriteLine($"Installed Plugin API: {AssemblyName.GetAssemblyName(apiPath).Version}");
    Console.WriteLine($"Plugin API file version: {FileVersionInfo.GetVersionInfo(apiPath).FileVersion}");
    Console.WriteLine("C# PluginApi.dll ships with Logi Plugin Service. Check Logi Options+ for host updates before development; this NuGet check cannot establish host freshness.");
    if (pinned != latest)
    {
        Console.Error.WriteLine("Update with: dotnet tool update logiplugintool --local, then rebuild and verify the package.");
        return 1;
    }
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"SDK verification failed: {exception.Message}");
    return 1;
}
