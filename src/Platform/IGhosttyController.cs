namespace Loupedeck.GhosttyPlugin.Platform;

public interface IGhosttyController
{
    Task ToggleSecureKeyboardEntryAsync();
    Task CopyAsync();
    Task CopyPlainTextAsync();
    Task CopyHtmlAsync();
    Task CopyTerminalFormattingAsync();
    Task CopyUrlAsync();
    Task CopyTerminalTitleAsync();
    Task PasteAsync();
    Task PasteSelectionAsync();
    Task OpenSettingsAsync();
    Task ReloadSettingsAsync();
    Task SplitLeftAsync();
    Task SplitRightAsync();
    Task SplitUpAsync();
    Task SplitDownAsync();
    Task NewTabAsync();
    Task NewWindowAsync();
    Task FocusSplitLeftAsync();
    Task FocusSplitRightAsync();
    Task FocusSplitUpAsync();
    Task FocusSplitDownAsync();
    Task ToggleSplitZoomAsync();
    Task EqualizeSplitsAsync();
    Task PreviousTabAsync();
    Task NextTabAsync();
    Task PreviousWindowAsync();
    Task NextWindowAsync();
    Task IncreaseFontSizeAsync();
    Task DecreaseFontSizeAsync();
    Task ResetFontSizeAsync();
    Task FindAsync();
    Task FindSelectionAsync();
    Task NextSearchResultAsync();
    Task PreviousSearchResultAsync();
    Task OpenScrollbackInEditorAsync();


}

public enum GhosttyOperation
{
    ToggleSecureKeyboardEntry,
    Copy,
    CopyPlainText,
    CopyHtml,
    CopyTerminalFormatting,
    CopyUrl,
    CopyTerminalTitle,
    Paste,
    PasteSelection,
    OpenSettings,
    ReloadSettings,
    SplitLeft,
    SplitRight,
    SplitUp,
    SplitDown,
    NewTab,
    NewWindow,
    FocusSplitLeft,
    FocusSplitRight,
    FocusSplitUp,
    FocusSplitDown,
    ToggleSplitZoom,
    EqualizeSplits,
    PreviousTab,
    NextTab,
    PreviousWindow,
    NextWindow,
    IncreaseFontSize,
    DecreaseFontSize,
    ResetFontSize,
    Find,
    FindSelection,
    NextSearchResult,
    PreviousSearchResult,
    OpenScrollbackInEditor,


}
