namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public abstract class SettingsCommandCommon : GhosttyCommandCommon
{
    protected SettingsCommandCommon(string name, string description,
        Func<IGhosttyController> createController = null, Action<Exception, string> logError = null)
        : base(name, description, "Settings", createController, logError) { }
}

public sealed class OpenSettingsCommand : SettingsCommandCommon
{
    public OpenSettingsCommand()
        : this(null, null) { }

    internal OpenSettingsCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Open Settings", "Open Ghostty configuration in the default editor", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.OpenSettingsAsync();
}

public sealed class ReloadSettingsCommand : SettingsCommandCommon
{
    public ReloadSettingsCommand()
        : this(null, null) { }

    internal ReloadSettingsCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Reload Settings", "Reload Ghostty configuration from disk", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.ReloadSettingsAsync();
}
