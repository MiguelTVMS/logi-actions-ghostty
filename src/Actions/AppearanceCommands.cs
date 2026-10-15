namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public sealed class IncreaseFontSizeCommand : GhosttyCommandCommon
{
    public IncreaseFontSizeCommand() : this(null, null) { }

    internal IncreaseFontSizeCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Increase Font Size", "Increase terminal font size by 1 point", "Appearance", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.IncreaseFontSizeAsync();
}

public sealed class DecreaseFontSizeCommand : GhosttyCommandCommon
{
    public DecreaseFontSizeCommand() : this(null, null) { }

    internal DecreaseFontSizeCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Decrease Font Size", "Decrease terminal font size by 1 point", "Appearance", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.DecreaseFontSizeAsync();
}

public sealed class ResetFontSizeCommand : GhosttyCommandCommon
{
    public ResetFontSizeCommand() : this(null, null) { }

    internal ResetFontSizeCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Reset Font Size", "Restore the configured terminal font size", "Appearance", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.ResetFontSizeAsync();
}
