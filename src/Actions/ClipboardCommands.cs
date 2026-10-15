namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public sealed class CopyCommand : GhosttyCommandCommon
{
    public CopyCommand() : this(null, null) { }

    internal CopyCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Copy", "Copy selected text in multiple clipboard formats", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.CopyAsync();
}

public sealed class CopyPlainTextCommand : GhosttyCommandCommon
{
    public CopyPlainTextCommand() : this(null, null) { }

    internal CopyPlainTextCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Copy as Plain Text", "Copy selected text without formatting", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.CopyPlainTextAsync();
}

public sealed class CopyHtmlCommand : GhosttyCommandCommon
{
    public CopyHtmlCommand() : this(null, null) { }

    internal CopyHtmlCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Copy as HTML", "Copy selected text with HTML formatting", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.CopyHtmlAsync();
}

public sealed class CopyTerminalFormattingCommand : GhosttyCommandCommon
{
    public CopyTerminalFormattingCommand() : this(null, null) { }

    internal CopyTerminalFormattingCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Copy with Terminal Formatting", "Copy selected text with terminal escape sequences", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.CopyTerminalFormattingAsync();
}

public sealed class CopyUrlCommand : GhosttyCommandCommon
{
    public CopyUrlCommand() : this(null, null) { }

    internal CopyUrlCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Copy URL", "Copy the URL under the terminal pointer", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.CopyUrlAsync();
}

public sealed class CopyTerminalTitleCommand : GhosttyCommandCommon
{
    public CopyTerminalTitleCommand() : this(null, null) { }

    internal CopyTerminalTitleCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Copy Terminal Title", "Copy the title of the focused terminal", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.CopyTerminalTitleAsync();
}

public sealed class PasteCommand : GhosttyCommandCommon
{
    public PasteCommand() : this(null, null) { }

    internal PasteCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Paste", "Paste from the system clipboard using Ghostty paste handling", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.PasteAsync();
}

public sealed class PasteSelectionCommand : GhosttyCommandCommon
{
    public PasteSelectionCommand() : this(null, null) { }

    internal PasteSelectionCommand(Func<IGhosttyController> controller, Action<Exception, string> logError)
        : base("Paste from Selection", "Paste from Ghostty selection clipboard", "Clipboard", controller, logError) { }

    protected override Task ExecuteAsync(IGhosttyController controller) => controller.PasteSelectionAsync();
}
