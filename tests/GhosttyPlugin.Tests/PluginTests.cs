using System.Diagnostics;
using System.Reflection;
using Loupedeck.GhosttyPlugin;
using Loupedeck.GhosttyPlugin.Platform;
using Xunit;

namespace GhosttyPlugin.Tests;

public class PluginTests
{
    [Fact]
    public async Task SettingsActionsDispatchAndLogFailures()
    {
        var controller = new FakeController();
        var errors = new List<Exception>();
        var open = new OpenSettingsCommand(() => controller, (e, _) => errors.Add(e));
        var reload = new ReloadSettingsCommand(() => controller, (e, _) => errors.Add(e));
        await open.ExecuteSafelyAsync();
        await reload.ExecuteSafelyAsync();
        Assert.Equal(1, controller.OpenCount);
        Assert.Equal(1, controller.ReloadCount);
        Assert.Empty(errors);
        controller.Fail = true;
        await open.ExecuteSafelyAsync();
        await reload.ExecuteSafelyAsync();
        Assert.Equal(2, errors.Count);
        var brokenFactory = new OpenSettingsCommand(() => throw new InvalidOperationException("factory"), (e, _) => errors.Add(e));
        await brokenFactory.ExecuteSafelyAsync();
        Assert.Equal("factory", errors.Last().Message);
    }

    [Fact]
    public void NativeActionEntryPointDispatchesWithoutBlocking()
    {
        var controller = new FakeController();
        var command = new OpenSettingsCommand(() => controller, (_, _) => { });
        typeof(SettingsCommandCommon).GetMethod("RunCommand", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(command, [""]);
        Assert.Equal(1, controller.OpenCount);
    }

    [Fact]
    public void PluginLifecycleAndActionMetadataAreValid()
    {
        var plugin = new Loupedeck.GhosttyPlugin.GhosttyPlugin();
        Assert.False(plugin.HasNoApplication);
        Assert.True(plugin.UsesApplicationApiOnly);
        plugin.Load();
        plugin.Unload();
        PluginLog.Error(new Exception("test"), "No host log is attached in unit tests.");
        Assert.Equal("Settings", new OpenSettingsCommand().GroupName);
        Assert.Equal("Settings", new ReloadSettingsCommand().GroupName);
    }

    [Fact]
    public void HostApplicationContractIsPresent()
    {
        var application = new GhosttyApplication();
        Assert.Equal(Loupedeck.ClientApplicationStatus.Unknown, application.GetApplicationStatus());
        var flags = BindingFlags.NonPublic | BindingFlags.Instance;
        Assert.Equal("", typeof(GhosttyApplication).GetMethod("GetProcessName", flags)!.Invoke(application, null));
        Assert.Equal("com.mitchellh.ghostty", typeof(GhosttyApplication).GetMethod("GetBundleName", flags)!.Invoke(application, null));
        var concreteApplications = typeof(GhosttyApplication).Assembly.GetTypes()
            .Where(type => !type.IsAbstract && typeof(Loupedeck.ClientApplication).IsAssignableFrom(type));
        Assert.Single(concreteApplications);
    }

    [Fact]
    public void FactoryCentralizesPlatformSelection()
    {
        Assert.IsType<GhosttyControllerMacOs>(GhosttyControllerFactory.Create(true, false));
        Assert.IsType<GhosttyControllerWindows>(GhosttyControllerFactory.Create(false, true));
        Assert.Throws<PlatformNotSupportedException>(() => GhosttyControllerFactory.Create(false, false));
        if (OperatingSystem.IsMacOS()) Assert.IsType<GhosttyControllerMacOs>(GhosttyControllerFactory.Create());
        else if (OperatingSystem.IsWindows()) Assert.IsType<GhosttyControllerWindows>(GhosttyControllerFactory.Create());
        else Assert.Throws<PlatformNotSupportedException>(() => GhosttyControllerFactory.Create());
    }

    [Fact]
    public async Task WindowsExplicitlyRejectsBothOperationsUntilSupported()
    {
        var controller = new GhosttyControllerWindows();
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => controller.OpenSettingsAsync());
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => controller.ReloadSettingsAsync());
    }

    [Theory]
    [InlineData(GhosttyOperation.OpenSettings, "open_config")]
    [InlineData(GhosttyOperation.ReloadSettings, "reload_config")]
    [InlineData(GhosttyOperation.SplitLeft, "new_split:left")]
    [InlineData(GhosttyOperation.SplitRight, "new_split:right")]
    [InlineData(GhosttyOperation.SplitUp, "new_split:up")]
    [InlineData(GhosttyOperation.SplitDown, "new_split:down")]
    [InlineData(GhosttyOperation.NewTab, "new_tab")]
    [InlineData(GhosttyOperation.FocusSplitLeft, "goto_split:left")]
    [InlineData(GhosttyOperation.FocusSplitRight, "goto_split:right")]
    [InlineData(GhosttyOperation.FocusSplitUp, "goto_split:up")]
    [InlineData(GhosttyOperation.FocusSplitDown, "goto_split:down")]
    [InlineData(GhosttyOperation.ToggleSplitZoom, "toggle_split_zoom")]
    [InlineData(GhosttyOperation.EqualizeSplits, "equalize_splits")]
    [InlineData(GhosttyOperation.PreviousTab, "previous_tab")]
    [InlineData(GhosttyOperation.NextTab, "next_tab")]
    [InlineData(GhosttyOperation.PreviousWindow, "goto_window:previous")]
    [InlineData(GhosttyOperation.NextWindow, "goto_window:next")]
    [InlineData(GhosttyOperation.IncreaseFontSize, "increase_font_size:1")]
    [InlineData(GhosttyOperation.DecreaseFontSize, "decrease_font_size:1")]
    [InlineData(GhosttyOperation.ResetFontSize, "reset_font_size")]
    [InlineData(GhosttyOperation.Find, "start_search")]
    [InlineData(GhosttyOperation.FindSelection, "search_selection")]
    [InlineData(GhosttyOperation.NextSearchResult, "navigate_search:next")]
    [InlineData(GhosttyOperation.PreviousSearchResult, "navigate_search:previous")]
    [InlineData(GhosttyOperation.OpenScrollbackInEditor, "write_scrollback_file:open")]
    [InlineData(GhosttyOperation.ToggleSecureKeyboardEntry, "toggle_secure_input")]
    [InlineData(GhosttyOperation.Copy, "copy_to_clipboard:mixed")]
    [InlineData(GhosttyOperation.CopyPlainText, "copy_to_clipboard:plain")]
    [InlineData(GhosttyOperation.CopyHtml, "copy_to_clipboard:html")]
    [InlineData(GhosttyOperation.CopyTerminalFormatting, "copy_to_clipboard:vt")]
    [InlineData(GhosttyOperation.CopyUrl, "copy_url_to_clipboard")]
    [InlineData(GhosttyOperation.CopyTerminalTitle, "copy_title_to_clipboard")]
    [InlineData(GhosttyOperation.Paste, "paste_from_clipboard")]
    [InlineData(GhosttyOperation.PasteSelection, "paste_from_selection")]
    public void MacOsUsesNativeActionArguments(GhosttyOperation operation, string action)
    {
        var info = CreateMacStartInfo(operation);
        Assert.Equal("/usr/bin/osascript", info.FileName);
        Assert.Equal("-e", info.ArgumentList[0]);
        Assert.Contains($"perform action \"{action}\" on targetTerminal", info.ArgumentList[1]);
        Assert.DoesNotContain("System Events", info.ArgumentList[1]);
        Assert.Contains("count of windows", info.ArgumentList[1]);
    }

    [Fact]
    public void NewWindowDoesNotRequireAnExistingTerminal()
    {
        var info = CreateMacStartInfo(GhosttyOperation.NewWindow);
        Assert.Equal("tell application \"Ghostty\" to new window", info.ArgumentList[1]);
    }

    [Fact]
    public void MacOsRejectsUnknownOperations()
    {
        var error = Assert.Throws<TargetInvocationException>(() => CreateMacStartInfo((GhosttyOperation)99));
        Assert.IsType<ArgumentOutOfRangeException>(error.InnerException);
    }

    [Fact]
    public async Task CommonRunnerHandlesExitStatusMissingExecutableAndTimeout()
    {
        await new ProcessController("exit 0").OpenSettingsAsync();
        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new ProcessController("echo failure >&2; exit 1").ReloadSettingsAsync());
        Assert.Contains("failure", error.Message);
        await Assert.ThrowsAsync<TimeoutException>(() =>
            new ProcessController("sleep 10", TimeSpan.FromMilliseconds(30)).OpenSettingsAsync());
        await Assert.ThrowsAsync<System.ComponentModel.Win32Exception>(() =>
            new ProcessController("", missing: true).OpenSettingsAsync());
    }

    [Fact]
    public async Task WindowActionsDispatchDirectionsAndRejectWindows()
    {
        var controller = new FakeController();
        var errors = new List<Exception>();
        await new SplitLeftCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        await new SplitRightCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        await new SplitUpCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        await new SplitDownCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        await new NewTabCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        await new NewWindowCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal(new[] { "SplitLeft", "SplitRight", "SplitUp", "SplitDown", "NewTab", "NewWindow" }, controller.WindowOperations);
        Assert.Empty(errors);
        Assert.Equal("Window Management", new SplitLeftCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().SplitLeftAsync());
        Assert.Equal("Window Management", new SplitRightCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().SplitRightAsync());
        Assert.Equal("Window Management", new SplitUpCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().SplitUpAsync());
        Assert.Equal("Window Management", new SplitDownCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().SplitDownAsync());
        Assert.Equal("Window Management", new NewTabCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().NewTabAsync());
        Assert.Equal("Window Management", new NewWindowCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().NewWindowAsync());
    }

    [Fact]
    public async Task RecommendedActionsDispatchAndRejectUnsupportedPlatform()
    {
        var controller = new FakeController();
        var errors = new List<Exception>();
        await new FocusSplitLeftCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new FocusSplitLeftCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().FocusSplitLeftAsync());
        await new FocusSplitRightCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new FocusSplitRightCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().FocusSplitRightAsync());
        await new FocusSplitUpCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new FocusSplitUpCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().FocusSplitUpAsync());
        await new FocusSplitDownCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new FocusSplitDownCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().FocusSplitDownAsync());
        await new ToggleSplitZoomCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new ToggleSplitZoomCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().ToggleSplitZoomAsync());
        await new EqualizeSplitsCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new EqualizeSplitsCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().EqualizeSplitsAsync());
        await new PreviousTabCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new PreviousTabCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().PreviousTabAsync());
        await new NextTabCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new NextTabCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().NextTabAsync());
        await new PreviousWindowCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new PreviousWindowCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().PreviousWindowAsync());
        await new NextWindowCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Window Management", new NextWindowCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().NextWindowAsync());
        await new IncreaseFontSizeCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Appearance", new IncreaseFontSizeCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().IncreaseFontSizeAsync());
        await new DecreaseFontSizeCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Appearance", new DecreaseFontSizeCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().DecreaseFontSizeAsync());
        await new ResetFontSizeCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Appearance", new ResetFontSizeCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().ResetFontSizeAsync());
        await new FindCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Search", new FindCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().FindAsync());
        await new FindSelectionCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Search", new FindSelectionCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().FindSelectionAsync());
        await new NextSearchResultCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Search", new NextSearchResultCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().NextSearchResultAsync());
        await new PreviousSearchResultCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Search", new PreviousSearchResultCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().PreviousSearchResultAsync());
        await new OpenScrollbackInEditorCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Terminal", new OpenScrollbackInEditorCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().OpenScrollbackInEditorAsync());
        Assert.Equal(new[] { "FocusSplitLeft", "FocusSplitRight", "FocusSplitUp", "FocusSplitDown", "ToggleSplitZoom", "EqualizeSplits", "PreviousTab", "NextTab", "PreviousWindow", "NextWindow", "IncreaseFontSize", "DecreaseFontSize", "ResetFontSize", "Find", "FindSelection", "NextSearchResult", "PreviousSearchResult", "OpenScrollbackInEditor" }, controller.WindowOperations);
        Assert.Empty(errors);
    }

    [Fact]
    public async Task SecureKeyboardEntryDispatchesAndRejectsUnsupportedPlatform()
    {
        var controller = new FakeController();
        var errors = new List<Exception>();
        await new ToggleSecureKeyboardEntryCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal(new[] { "ToggleSecureKeyboardEntry" }, controller.WindowOperations);
        Assert.Empty(errors);
        Assert.Equal("Terminal", new ToggleSecureKeyboardEntryCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().ToggleSecureKeyboardEntryAsync());
    }

    [Fact]
    public async Task ClipboardActionsDispatchFormatsAndRejectUnsupportedPlatform()
    {
        var controller = new FakeController();
        var errors = new List<Exception>();
        await new CopyCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new CopyCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().CopyAsync());
        await new CopyPlainTextCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new CopyPlainTextCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().CopyPlainTextAsync());
        await new CopyHtmlCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new CopyHtmlCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().CopyHtmlAsync());
        await new CopyTerminalFormattingCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new CopyTerminalFormattingCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().CopyTerminalFormattingAsync());
        await new CopyUrlCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new CopyUrlCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().CopyUrlAsync());
        await new CopyTerminalTitleCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new CopyTerminalTitleCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().CopyTerminalTitleAsync());
        await new PasteCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new PasteCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().PasteAsync());
        await new PasteSelectionCommand(() => controller, (e, _) => errors.Add(e)).ExecuteSafelyAsync();
        Assert.Equal("Clipboard", new PasteSelectionCommand().GroupName);
        await Assert.ThrowsAsync<PlatformNotSupportedException>(() => new GhosttyControllerWindows().PasteSelectionAsync());
        Assert.Equal(new[] { "Copy", "CopyPlainText", "CopyHtml", "CopyTerminalFormatting", "CopyUrl", "CopyTerminalTitle", "Paste", "PasteSelection" }, controller.WindowOperations);
        Assert.Empty(errors);
    }

    private static ProcessStartInfo CreateMacStartInfo(GhosttyOperation operation) =>
        (ProcessStartInfo)typeof(GhosttyControllerMacOs)
            .GetMethod("CreateStartInfo", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(new GhosttyControllerMacOs(), [operation])!;

    private sealed class FakeController : IGhosttyController
    {
        public int OpenCount { get; private set; }
        public int ReloadCount { get; private set; }
        public List<string> WindowOperations { get; } = [];
        public Task SplitLeftAsync() { WindowOperations.Add("SplitLeft"); return Task.CompletedTask; }
        public Task SplitRightAsync() { WindowOperations.Add("SplitRight"); return Task.CompletedTask; }
        public Task SplitUpAsync() { WindowOperations.Add("SplitUp"); return Task.CompletedTask; }
        public Task SplitDownAsync() { WindowOperations.Add("SplitDown"); return Task.CompletedTask; }
        public Task NewTabAsync() { WindowOperations.Add("NewTab"); return Task.CompletedTask; }
        public Task NewWindowAsync() { WindowOperations.Add("NewWindow"); return Task.CompletedTask; }
        public Task FocusSplitLeftAsync() { WindowOperations.Add("FocusSplitLeft"); return Task.CompletedTask; }
        public Task FocusSplitRightAsync() { WindowOperations.Add("FocusSplitRight"); return Task.CompletedTask; }
        public Task FocusSplitUpAsync() { WindowOperations.Add("FocusSplitUp"); return Task.CompletedTask; }
        public Task FocusSplitDownAsync() { WindowOperations.Add("FocusSplitDown"); return Task.CompletedTask; }
        public Task ToggleSplitZoomAsync() { WindowOperations.Add("ToggleSplitZoom"); return Task.CompletedTask; }
        public Task EqualizeSplitsAsync() { WindowOperations.Add("EqualizeSplits"); return Task.CompletedTask; }
        public Task PreviousTabAsync() { WindowOperations.Add("PreviousTab"); return Task.CompletedTask; }
        public Task NextTabAsync() { WindowOperations.Add("NextTab"); return Task.CompletedTask; }
        public Task PreviousWindowAsync() { WindowOperations.Add("PreviousWindow"); return Task.CompletedTask; }
        public Task NextWindowAsync() { WindowOperations.Add("NextWindow"); return Task.CompletedTask; }
        public Task IncreaseFontSizeAsync() { WindowOperations.Add("IncreaseFontSize"); return Task.CompletedTask; }
        public Task DecreaseFontSizeAsync() { WindowOperations.Add("DecreaseFontSize"); return Task.CompletedTask; }
        public Task ResetFontSizeAsync() { WindowOperations.Add("ResetFontSize"); return Task.CompletedTask; }
        public Task FindAsync() { WindowOperations.Add("Find"); return Task.CompletedTask; }
        public Task FindSelectionAsync() { WindowOperations.Add("FindSelection"); return Task.CompletedTask; }
        public Task NextSearchResultAsync() { WindowOperations.Add("NextSearchResult"); return Task.CompletedTask; }
        public Task PreviousSearchResultAsync() { WindowOperations.Add("PreviousSearchResult"); return Task.CompletedTask; }
        public Task OpenScrollbackInEditorAsync() { WindowOperations.Add("OpenScrollbackInEditor"); return Task.CompletedTask; }
        public Task ToggleSecureKeyboardEntryAsync() { WindowOperations.Add("ToggleSecureKeyboardEntry"); return Task.CompletedTask; }
        public Task CopyAsync() { WindowOperations.Add("Copy"); return Task.CompletedTask; }
        public Task CopyPlainTextAsync() { WindowOperations.Add("CopyPlainText"); return Task.CompletedTask; }
        public Task CopyHtmlAsync() { WindowOperations.Add("CopyHtml"); return Task.CompletedTask; }
        public Task CopyTerminalFormattingAsync() { WindowOperations.Add("CopyTerminalFormatting"); return Task.CompletedTask; }
        public Task CopyUrlAsync() { WindowOperations.Add("CopyUrl"); return Task.CompletedTask; }
        public Task CopyTerminalTitleAsync() { WindowOperations.Add("CopyTerminalTitle"); return Task.CompletedTask; }
        public Task PasteAsync() { WindowOperations.Add("Paste"); return Task.CompletedTask; }
        public Task PasteSelectionAsync() { WindowOperations.Add("PasteSelection"); return Task.CompletedTask; }
        public bool Fail { get; set; }
        public Task OpenSettingsAsync()
        {
            OpenCount++;
            return Fail ? Task.FromException(new InvalidOperationException("open")) : Task.CompletedTask;
        }
        public Task ReloadSettingsAsync()
        {
            ReloadCount++;
            return Fail ? Task.FromException(new InvalidOperationException("reload")) : Task.CompletedTask;
        }
    }

    private sealed class ProcessController(string script, TimeSpan? timeout = null, bool missing = false)
        : GhosttyControllerCommon(timeout)
    {
        protected override ProcessStartInfo CreateStartInfo(GhosttyOperation operation)
        {
            var info = new ProcessStartInfo(missing ? "/nonexistent/ghostty-test-process" : "/bin/sh");
            info.ArgumentList.Add("-c");
            info.ArgumentList.Add(script);
            return info;
        }
    }
}
