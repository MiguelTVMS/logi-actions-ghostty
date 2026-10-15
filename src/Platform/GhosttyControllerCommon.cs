namespace Loupedeck.GhosttyPlugin.Platform;

using System.Diagnostics;

public abstract class GhosttyControllerCommon : IGhosttyController
{
    private readonly TimeSpan _timeout;

    protected GhosttyControllerCommon(TimeSpan? timeout = null) =>
        _timeout = timeout ?? TimeSpan.FromSeconds(60);

    public Task ToggleSecureKeyboardEntryAsync() => ExecuteAsync(GhosttyOperation.ToggleSecureKeyboardEntry);

    public Task OpenSettingsAsync() => ExecuteAsync(GhosttyOperation.OpenSettings);
    public Task ReloadSettingsAsync() => ExecuteAsync(GhosttyOperation.ReloadSettings);

    public Task SplitLeftAsync() => ExecuteAsync(GhosttyOperation.SplitLeft);
    public Task SplitRightAsync() => ExecuteAsync(GhosttyOperation.SplitRight);
    public Task SplitUpAsync() => ExecuteAsync(GhosttyOperation.SplitUp);
    public Task SplitDownAsync() => ExecuteAsync(GhosttyOperation.SplitDown);
    public Task NewTabAsync() => ExecuteAsync(GhosttyOperation.NewTab);
    public Task NewWindowAsync() => ExecuteAsync(GhosttyOperation.NewWindow);

    public Task FocusSplitLeftAsync() => ExecuteAsync(GhosttyOperation.FocusSplitLeft);
    public Task FocusSplitRightAsync() => ExecuteAsync(GhosttyOperation.FocusSplitRight);
    public Task FocusSplitUpAsync() => ExecuteAsync(GhosttyOperation.FocusSplitUp);
    public Task FocusSplitDownAsync() => ExecuteAsync(GhosttyOperation.FocusSplitDown);
    public Task ToggleSplitZoomAsync() => ExecuteAsync(GhosttyOperation.ToggleSplitZoom);
    public Task EqualizeSplitsAsync() => ExecuteAsync(GhosttyOperation.EqualizeSplits);
    public Task PreviousTabAsync() => ExecuteAsync(GhosttyOperation.PreviousTab);
    public Task NextTabAsync() => ExecuteAsync(GhosttyOperation.NextTab);
    public Task PreviousWindowAsync() => ExecuteAsync(GhosttyOperation.PreviousWindow);
    public Task NextWindowAsync() => ExecuteAsync(GhosttyOperation.NextWindow);
    public Task IncreaseFontSizeAsync() => ExecuteAsync(GhosttyOperation.IncreaseFontSize);
    public Task DecreaseFontSizeAsync() => ExecuteAsync(GhosttyOperation.DecreaseFontSize);
    public Task ResetFontSizeAsync() => ExecuteAsync(GhosttyOperation.ResetFontSize);
    public Task FindAsync() => ExecuteAsync(GhosttyOperation.Find);
    public Task FindSelectionAsync() => ExecuteAsync(GhosttyOperation.FindSelection);
    public Task NextSearchResultAsync() => ExecuteAsync(GhosttyOperation.NextSearchResult);
    public Task PreviousSearchResultAsync() => ExecuteAsync(GhosttyOperation.PreviousSearchResult);
    public Task OpenScrollbackInEditorAsync() => ExecuteAsync(GhosttyOperation.OpenScrollbackInEditor);

    public Task CopyAsync() => ExecuteAsync(GhosttyOperation.Copy);
    public Task CopyPlainTextAsync() => ExecuteAsync(GhosttyOperation.CopyPlainText);
    public Task CopyHtmlAsync() => ExecuteAsync(GhosttyOperation.CopyHtml);
    public Task CopyTerminalFormattingAsync() => ExecuteAsync(GhosttyOperation.CopyTerminalFormatting);
    public Task CopyUrlAsync() => ExecuteAsync(GhosttyOperation.CopyUrl);
    public Task CopyTerminalTitleAsync() => ExecuteAsync(GhosttyOperation.CopyTerminalTitle);
    public Task PasteAsync() => ExecuteAsync(GhosttyOperation.Paste);
    public Task PasteSelectionAsync() => ExecuteAsync(GhosttyOperation.PasteSelection);

    private async Task ExecuteAsync(GhosttyOperation operation)
    {
        var startInfo = CreateStartInfo(operation);
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardError = true;
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start the Ghostty automation process.");
        var errorTask = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(_timeout);
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            process.Kill(entireProcessTree: true);
            throw new TimeoutException("Ghostty automation timed out. Check macOS Automation permissions.");
        }
        var error = await errorTask;
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"Ghostty {operation} failed: {error.Trim()}");
        }
    }

    protected abstract ProcessStartInfo CreateStartInfo(GhosttyOperation operation);
}
