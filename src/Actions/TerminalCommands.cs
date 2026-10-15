namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public sealed class OpenScrollbackInEditorCommand : GhosttyCommandCommon
{
    public OpenScrollbackInEditorCommand() : this(null, null) { }

    internal OpenScrollbackInEditorCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Open Scrollback in Editor", "Open terminal scrollback in the default text editor", "Terminal", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.OpenScrollbackInEditorAsync();
}

public sealed class ToggleSecureKeyboardEntryCommand : GhosttyCommandCommon
{
    public ToggleSecureKeyboardEntryCommand() : this(null, null) { }

    internal ToggleSecureKeyboardEntryCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Toggle Secure Keyboard Entry", "Toggle the persistent secure keyboard entry setting for all Ghostty windows", "Terminal", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.ToggleSecureKeyboardEntryAsync();
}
