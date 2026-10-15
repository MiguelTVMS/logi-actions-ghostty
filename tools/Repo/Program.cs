using System.Diagnostics;
using System.Reflection;
using System.Xml.Linq;
using Loupedeck;
using Loupedeck.GhosttyPlugin;

try
{
    var command = args.FirstOrDefault() ?? "check";
    switch (command)
    {
        case "check":
            await Run("dotnet", "run", "--project", "tools/SdkCheck");
            await Run("python3", "-m", "unittest", "discover", "-s", "tools/release", "-p", "test_*.py");
            Translations();
            Icons();
            await Coverage();
            await Run("dotnet", "build", "GhosttyPlugin.sln", "-c", "Release", "-p:RestoreLockedMode=true");
            await Run("dotnet", "tool", "run", "logiplugintool", "--", "pack", "bin/Release", PackagePath());
            await Run("dotnet", "tool", "run", "logiplugintool", "--", "verify", PackagePath());
            break;
        case "release-version":
            if (args.Length != 2) throw new InvalidOperationException("Usage: ./repo release-version Major|Minor|Fix");
            CheckBranch();
            await Run("python3", "tools/release/version.py", args[1]);
            break;
        case "test":
            await Run("dotnet", "test", "tests/GhosttyPlugin.Tests", "-c", "Release", "-p:RestoreLockedMode=true");
            break;
        case "coverage": await Coverage(); break;
        case "translations": Translations(); break;
        case "icons": Icons(); break;
        case "profile":
            if (args.Length != 2) throw new InvalidOperationException("Usage: ./repo profile <exported-file.lp5>");
            CopyProfile(args[1]);
            break;
        case "precommit":
            CheckBranch();
            await Run("bash", "./repo", "check");
            break;
        case "prepush":
            CheckPushRefs();
            await Run("bash", "./repo", "check");
            break;
        case "hooks":
            await Run("git", "config", "--local", "core.hooksPath", ".githooks");
            Console.WriteLine("Repository Git hooks enabled.");
            break;
        default: throw new InvalidOperationException("Commands: check, test, coverage, translations, icons, release-version Major|Minor|Fix, profile <exported-file.lp5>, precommit, prepush, hooks");
    }
    return 0;
}
catch (Exception exception)
{
    Console.Error.WriteLine($"Validation failed: {exception.Message}");
    return 1;
}

static string PackagePath()
{
    var version = XDocument.Load("src/GhosttyPlugin.csproj").Descendants("Version").Single().Value;
    if (!System.Text.RegularExpressions.Regex.IsMatch(version, @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)$"))
        throw new InvalidOperationException("Plugin version must be numeric major.minor.patch.");
    var metadata = File.ReadAllLines("src/package/metadata/LoupedeckPackage.yaml").Single(line => line.StartsWith("version: ", StringComparison.Ordinal));
    if (metadata != $"version: {version}") throw new InvalidOperationException("Plugin assembly and package versions differ.");
    return $"Ghostty_{version}.lplug4";
}

static async Task Coverage()
{
    Directory.CreateDirectory("artifacts/coverage");
    // Remove old reports so a failed or empty run can never pass using stale results.
    foreach (var path in Directory.GetFiles("artifacts/coverage", "*.xml")) File.Delete(path);
    await Run("dotnet", "test", "tests/GhosttyPlugin.Tests", "-c", "Release", "-p:RestoreLockedMode=true",
        "-p:CollectCoverage=true", "-p:CoverletOutputFormat=cobertura",
        "-p:CoverletOutput=../../artifacts/coverage/", "-p:Include=[GhosttyPlugin]*",
        "-p:Threshold=90", "-p:ThresholdType=line", "-p:ThresholdStat=total");
    var report = XDocument.Load("artifacts/coverage/coverage.cobertura.xml").Root!;
    var valid = int.Parse(report.Attribute("lines-valid")!.Value);
    var covered = int.Parse(report.Attribute("lines-covered")!.Value);
    if (valid == 0 || 100m * covered / valid < 90m)
        throw new InvalidOperationException("Production line coverage must be at least 90%, with a nonempty report.");
    Console.WriteLine($"Production line coverage: {100m * covered / valid:F2}% ({covered}/{valid}).");
}

static void Translations()
{
    var commands = typeof(OpenSettingsCommand).Assembly.GetTypes()
        .Where(type => !type.IsAbstract && typeof(PluginDynamicCommand).IsAssignableFrom(type))
        .Select(type => (PluginDynamicCommand)Activator.CreateInstance(type)!).ToArray();
    var expected = new Dictionary<string, string>();
    var expectedGroups = new HashSet<string>();
    foreach (var action in commands)
    {
        expected.Add($"$Ghostty___{action.Name}", action.DisplayName);
        expected.Add($"$Ghostty___{action.Name}###description", action.Description);
        if (!string.IsNullOrWhiteSpace(action.GroupName))
            expectedGroups.Add(action.GroupName);
    }
    if (expected.Count == 0) throw new InvalidOperationException("No action strings were discovered.");
    foreach (var language in new[] { "en-US", "pt-BR" })
    {
        var path = $"src/package/localization/Ghostty_{language}.xliff";
        var xml = XDocument.Load(path);
        if (xml.Root?.Name.LocalName != "xliff" || xml.Root.Attribute("version")?.Value != "1.2")
            throw new InvalidOperationException($"Invalid XLIFF document: {path}");
        foreach (var file in xml.Root.Elements("file"))
            if ((string?)file.Attribute("source-language") != "en-US" || (string?)file.Attribute("target-language") != language)
                throw new InvalidOperationException($"Wrong source or target language in {path}");
        var units = xml.Descendants("trans-unit").ToDictionary(unit => (string)unit.Attribute("id")!);
        var groups = xml.Descendants("group").Where(group => (string?)group.Attribute("id") == "@groups")
            .Elements("trans-unit").ToArray();
        if (!groups.Select(group => group.Element("source")?.Value ?? "").ToHashSet().SetEquals(expectedGroups))
            throw new InvalidOperationException($"Missing or obsolete action groups in {path}");
        var allExpected = new Dictionary<string, string>(expected);
        foreach (var group in groups)
        {
            var id = (string?)group.Attribute("id");
            if (string.IsNullOrWhiteSpace(id) || !id.StartsWith("$Ghostty___GroupName___", StringComparison.Ordinal))
                throw new InvalidOperationException($"Invalid group localization ID in {path}");
            // Group hashes differ by SDK release; runtime localization is keyed by English source.
            allExpected.Add(id, group.Element("source")!.Value);
        }
        if (!units.Keys.ToHashSet().SetEquals(allExpected.Keys))
            throw new InvalidOperationException($"Missing or obsolete translation IDs in {path}");
        // Use the real SDK parser as well as structural checks.
        var dictionary = new XliffDictionary().ReadFile(path);
        foreach (var (id, source) in allExpected)
        {
            var unit = units[id];
            var target = unit.Element("target")?.Value;
            if (unit.Element("source")?.Value != source || string.IsNullOrWhiteSpace(target))
                throw new InvalidOperationException($"Missing/stale translation '{source}' in {path}");
            if (language == "en-US" && target != source)
                throw new InvalidOperationException($"English source and target must match for '{source}'.");
            if (dictionary[source] != target)
                throw new InvalidOperationException($"SDK could not resolve '{source}' in {path}");
        }
        Console.WriteLine($"{language}: 100% translation coverage ({allExpected.Count}/{allExpected.Count}).");
    }
}

static void Icons()
{
    var actions = typeof(OpenSettingsCommand).Assembly.GetTypes()
        .Where(type => !type.IsAbstract && typeof(PluginDynamicCommand).IsAssignableFrom(type)).ToArray();
    XNamespace ns = "http://www.w3.org/2000/svg";
    foreach (var action in actions)
    {
        var path = $"src/package/actionsymbols/{action.FullName}.svg";
        var svg = XDocument.Load(path).Root!;
        if (svg.Name != ns + "svg" || (string?)svg.Attribute("fill") != "none" || svg.Attribute("viewBox") is null)
            throw new InvalidOperationException($"Action icon must be an outlined SVG with viewBox: {path}");
        if (!svg.Descendants().Any(element => element.Name == ns + "path"))
            throw new InvalidOperationException($"Action SVG has no paths: {path}");
        foreach (var element in svg.DescendantsAndSelf())
        {
            if (element.Name.LocalName is "rect" or "image" or "script" or "foreignObject" or "linearGradient" or "radialGradient" ||
                element.Attributes().Any(attribute => attribute.Name.LocalName is "href" or "style" ||
                    (attribute.Name.LocalName == "fill" && attribute.Value is not ("none" or "#000000" or "black")) ||
                    (attribute.Name.LocalName == "stroke" && attribute.Value is not ("#000000" or "black"))))
                throw new InvalidOperationException($"Action icon must use transparent, monochrome outline paths: {path}");
        }
        var templatePath = $"src/package/icontemplates/{action.FullName}.ict";
        using var template = System.Text.Json.JsonDocument.Parse(File.ReadAllText(templatePath));
        if (template.RootElement.GetProperty("backgroundColor").GetUInt32() != 0xFF149CFF)
            throw new InvalidOperationException($"Action icon template must use the default Ghostty-inspired blue background: {templatePath}");
        var imageItem = template.RootElement.GetProperty("items").EnumerateArray()
            .Single(item => item.GetProperty("itemType").GetString() == "Image");
        if (imageItem.GetProperty("imageColor").GetUInt32() != 0xFFFFFFFF)
            throw new InvalidOperationException($"Action icon template must use white outlines: {templatePath}");
        var expectedButtonSvg = File.ReadAllText(path).Replace("#000000", "#FFFFFF", StringComparison.Ordinal);
        var buttonSvg = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(imageItem.GetProperty("image").GetString()!));
        if (buttonSvg != expectedButtonSvg)
            throw new InvalidOperationException($"Button icon must match its symbol with white default strokes: {templatePath}");
    }
    Console.WriteLine($"Action icon coverage: 100% ({actions.Length}/{actions.Length} SVG symbols and templates).");
}

static void CopyProfile(string source)
{
    var sourcePath = Path.GetFullPath(source);
    if (!File.Exists(sourcePath) || !string.Equals(Path.GetExtension(sourcePath), ".lp5", StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Choose an existing .lp5 file exported from Logi Options+.");
    using (var archive = System.IO.Compression.ZipFile.OpenRead(sourcePath))
    {
        System.Text.Json.JsonDocument ReadJson(string name)
        {
            var entry = archive.GetEntry(name) ?? throw new InvalidOperationException($"The exported profile is missing {name}.");
            using var stream = entry.Open();
            return System.Text.Json.JsonDocument.Parse(stream);
        }
        using var application = ReadJson("ApplicationInfo.json");
        using var profile = ReadJson("ProfileInfo.json");
        var app = application.RootElement;
        var data = profile.RootElement;
        if (app.GetProperty("deviceType").GetString() != "Loupedeck72" || data.GetProperty("deviceType").GetString() != "Loupedeck72")
            throw new InvalidOperationException("The bundled default must be an Actions Ring profile.");
        var isGhostty = (app.TryGetProperty("nativePluginName", out var native) && native.GetString() == "Ghostty") ||
            (app.TryGetProperty("processOrBundleName", out var bundle) && bundle.GetString() == "com.mitchellh.ghostty");
        if (!isGhostty) throw new InvalidOperationException("Export the Ghostty application profile, not another application's profile.");
        var profileJson = data.GetRawText();
        foreach (var action in new[] { typeof(OpenSettingsCommand), typeof(ReloadSettingsCommand) })
            if (!profileJson.Contains($"$Ghostty___{action.FullName}", StringComparison.Ordinal))
                throw new InvalidOperationException($"The profile does not contain {action.Name}. Add both Settings actions and export again.");
        if (profileJson.Contains("/Users/", StringComparison.Ordinal) || profileJson.Contains("C:\\Users\\", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("The profile contains a user-specific filesystem path. Remove personal action paths in Options+ and export again.");
    }
    var destination = Path.GetFullPath("src/package/profiles/DefaultProfile72.lp5");
    if (sourcePath == destination) throw new InvalidOperationException("The selected file is already the bundled default.");
    Directory.CreateDirectory(Path.GetDirectoryName(destination)!);
    File.Copy(sourcePath, destination, overwrite: true);
    Console.WriteLine("Copied authentic export to src/package/profiles/DefaultProfile72.lp5 (contents unchanged).");
    Console.WriteLine("Run ./repo check to validate and package the updated default.");
}

static void CheckBranch()
{
    var branch = Capture("git", "branch", "--show-current");
    if (!new[] { "feature/", "release/", "hotfix/" }.Any(prefix => branch.StartsWith(prefix, StringComparison.Ordinal) && branch.Length > prefix.Length))
        throw new InvalidOperationException("Commit on a Gitflow feature/*, release/* or hotfix/* branch. develop/main require PRs.");
}

static void CheckPushRefs()
{
    string? line;
    while ((line = Console.ReadLine()) is not null)
    {
        var fields = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (fields.Length != 4) throw new InvalidOperationException("Invalid pre-push input.");
        var localRef = fields[0];
        var remoteRef = fields[2];
        if (fields[1].All(character => character == '0')) continue; // Branch deletion.
        if (remoteRef is "refs/heads/develop" or "refs/heads/main")
            throw new InvalidOperationException("Direct pushes to develop/main are forbidden. Use a pull request.");
        if (remoteRef.StartsWith("refs/tags/", StringComparison.Ordinal))
        {
            var branch = Capture("git", "branch", "--show-current");
            if (!branch.StartsWith("release/", StringComparison.Ordinal))
                throw new InvalidOperationException("Release tags must be pushed from a release/* branch.");
            var taggedCommit = Capture("git", "rev-parse", $"{fields[1]}^{{commit}}");
            if (taggedCommit != Capture("git", "rev-parse", "HEAD"))
                throw new InvalidOperationException("Release tags must identify the current validated release commit.");
            continue;
        }
        var branchName = remoteRef.Replace("refs/heads/", "");
        if (!new[] { "feature/", "release/", "hotfix/" }.Any(prefix => branchName.StartsWith(prefix, StringComparison.Ordinal) && branchName.Length > prefix.Length))
            throw new InvalidOperationException($"Use a Gitflow feature/*, release/* or hotfix/* branch: {localRef}");
        if (fields[1] != Capture("git", "rev-parse", "HEAD"))
            throw new InvalidOperationException("Only the checked-out commit can be pushed after validation.");
    }
    if (Capture("git", "status", "--porcelain").Length != 0)
        throw new InvalidOperationException("Commit or stash working tree changes before pushing so checks validate the pushed commit.");
}

static string Capture(string executable, params string[] arguments)
{
    var info = new ProcessStartInfo(executable) { RedirectStandardOutput = true, UseShellExecute = false };
    foreach (var argument in arguments) info.ArgumentList.Add(argument);
    using var process = Process.Start(info)!;
    var output = process.StandardOutput.ReadToEnd();
    process.WaitForExit();
    if (process.ExitCode != 0) throw new InvalidOperationException($"{executable} failed.");
    return output.Trim();
}

static async Task Run(string executable, params string[] arguments)
{
    Console.WriteLine($"> {executable} {string.Join(' ', arguments)}");
    var info = new ProcessStartInfo(executable) { UseShellExecute = false };
    foreach (var argument in arguments) info.ArgumentList.Add(argument);
    using var process = Process.Start(info)!;
    await process.WaitForExitAsync();
    if (process.ExitCode != 0) throw new InvalidOperationException($"Command exited with {process.ExitCode}.");
}
