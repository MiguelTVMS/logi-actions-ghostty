namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public sealed class SplitLeftCommand : GhosttyCommandCommon
{
    public SplitLeftCommand() : this(null, null) { }

    internal SplitLeftCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Split Left", "Create a split to the left", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.SplitLeftAsync();
}

public sealed class SplitRightCommand : GhosttyCommandCommon
{
    public SplitRightCommand() : this(null, null) { }

    internal SplitRightCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Split Right", "Create a split to the right", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.SplitRightAsync();
}

public sealed class SplitUpCommand : GhosttyCommandCommon
{
    public SplitUpCommand() : this(null, null) { }

    internal SplitUpCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Split Up", "Create a split above", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.SplitUpAsync();
}

public sealed class SplitDownCommand : GhosttyCommandCommon
{
    public SplitDownCommand() : this(null, null) { }

    internal SplitDownCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Split Down", "Create a split below", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.SplitDownAsync();
}

public sealed class NewTabCommand : GhosttyCommandCommon
{
    public NewTabCommand() : this(null, null) { }

    internal NewTabCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("New Tab", "Open a new Ghostty tab", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.NewTabAsync();
}

public sealed class NewWindowCommand : GhosttyCommandCommon
{
    public NewWindowCommand() : this(null, null) { }

    internal NewWindowCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("New Window", "Open a new Ghostty window", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.NewWindowAsync();
}

public sealed class FocusSplitLeftCommand : GhosttyCommandCommon
{
    public FocusSplitLeftCommand() : this(null, null) { }

    internal FocusSplitLeftCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Focus Split Left", "Focus Split Left in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.FocusSplitLeftAsync();
}

public sealed class FocusSplitRightCommand : GhosttyCommandCommon
{
    public FocusSplitRightCommand() : this(null, null) { }

    internal FocusSplitRightCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Focus Split Right", "Focus Split Right in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.FocusSplitRightAsync();
}

public sealed class FocusSplitUpCommand : GhosttyCommandCommon
{
    public FocusSplitUpCommand() : this(null, null) { }

    internal FocusSplitUpCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Focus Split Up", "Focus Split Up in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.FocusSplitUpAsync();
}

public sealed class FocusSplitDownCommand : GhosttyCommandCommon
{
    public FocusSplitDownCommand() : this(null, null) { }

    internal FocusSplitDownCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Focus Split Down", "Focus Split Down in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.FocusSplitDownAsync();
}

public sealed class ToggleSplitZoomCommand : GhosttyCommandCommon
{
    public ToggleSplitZoomCommand() : this(null, null) { }

    internal ToggleSplitZoomCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Toggle Split Zoom", "Toggle Split Zoom in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.ToggleSplitZoomAsync();
}

public sealed class EqualizeSplitsCommand : GhosttyCommandCommon
{
    public EqualizeSplitsCommand() : this(null, null) { }

    internal EqualizeSplitsCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Equalize Splits", "Equalize Splits in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.EqualizeSplitsAsync();
}

public sealed class PreviousTabCommand : GhosttyCommandCommon
{
    public PreviousTabCommand() : this(null, null) { }

    internal PreviousTabCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Previous Tab", "Previous Tab in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.PreviousTabAsync();
}

public sealed class NextTabCommand : GhosttyCommandCommon
{
    public NextTabCommand() : this(null, null) { }

    internal NextTabCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Next Tab", "Next Tab in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.NextTabAsync();
}

public sealed class PreviousWindowCommand : GhosttyCommandCommon
{
    public PreviousWindowCommand() : this(null, null) { }

    internal PreviousWindowCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Previous Window", "Previous Window in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.PreviousWindowAsync();
}

public sealed class NextWindowCommand : GhosttyCommandCommon
{
    public NextWindowCommand() : this(null, null) { }

    internal NextWindowCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Next Window", "Next Window in the focused Ghostty terminal", "Window Management", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.NextWindowAsync();
}
