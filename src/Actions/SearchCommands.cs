namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public sealed class FindCommand : GhosttyCommandCommon
{
    public FindCommand() : this(null, null) { }

    internal FindCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Find", "Open terminal search", "Search", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.FindAsync();
}

public sealed class FindSelectionCommand : GhosttyCommandCommon
{
    public FindSelectionCommand() : this(null, null) { }

    internal FindSelectionCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Find Selection", "Search terminal output for the selected text", "Search", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.FindSelectionAsync();
}

public sealed class NextSearchResultCommand : GhosttyCommandCommon
{
    public NextSearchResultCommand() : this(null, null) { }

    internal NextSearchResultCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Next Search Result", "Next Search Result in the focused Ghostty terminal", "Search", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.NextSearchResultAsync();
}

public sealed class PreviousSearchResultCommand : GhosttyCommandCommon
{
    public PreviousSearchResultCommand() : this(null, null) { }

    internal PreviousSearchResultCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Previous Search Result", "Previous Search Result in the focused Ghostty terminal", "Search", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.PreviousSearchResultAsync();
}
