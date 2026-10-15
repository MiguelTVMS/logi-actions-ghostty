namespace Loupedeck.GhosttyPlugin.Platform;

using System.Diagnostics;

public sealed class GhosttyControllerMacOs : GhosttyControllerCommon
{
    protected override ProcessStartInfo CreateStartInfo(GhosttyOperation operation)
    {
        var action = operation switch
        {
            GhosttyOperation.ToggleSecureKeyboardEntry => "toggle_secure_input",
            GhosttyOperation.OpenSettings => "open_config",
            GhosttyOperation.ReloadSettings => "reload_config",
            GhosttyOperation.SplitLeft => "new_split:left",
            GhosttyOperation.SplitRight => "new_split:right",
            GhosttyOperation.SplitUp => "new_split:up",
            GhosttyOperation.SplitDown => "new_split:down",
            GhosttyOperation.NewTab => "new_tab",
            GhosttyOperation.NewWindow => "new_window",
            GhosttyOperation.FocusSplitLeft => "goto_split:left",
            GhosttyOperation.FocusSplitRight => "goto_split:right",
            GhosttyOperation.FocusSplitUp => "goto_split:up",
            GhosttyOperation.FocusSplitDown => "goto_split:down",
            GhosttyOperation.ToggleSplitZoom => "toggle_split_zoom",
            GhosttyOperation.EqualizeSplits => "equalize_splits",
            GhosttyOperation.PreviousTab => "previous_tab",
            GhosttyOperation.NextTab => "next_tab",
            GhosttyOperation.PreviousWindow => "goto_window:previous",
            GhosttyOperation.NextWindow => "goto_window:next",
            GhosttyOperation.IncreaseFontSize => "increase_font_size:1",
            GhosttyOperation.DecreaseFontSize => "decrease_font_size:1",
            GhosttyOperation.ResetFontSize => "reset_font_size",
            GhosttyOperation.Find => "start_search",
            GhosttyOperation.FindSelection => "search_selection",
            GhosttyOperation.NextSearchResult => "navigate_search:next",
            GhosttyOperation.PreviousSearchResult => "navigate_search:previous",
            GhosttyOperation.OpenScrollbackInEditor => "write_scrollback_file:open",
            GhosttyOperation.Copy => "copy_to_clipboard:mixed",
            GhosttyOperation.CopyPlainText => "copy_to_clipboard:plain",
            GhosttyOperation.CopyHtml => "copy_to_clipboard:html",
            GhosttyOperation.CopyTerminalFormatting => "copy_to_clipboard:vt",
            GhosttyOperation.CopyUrl => "copy_url_to_clipboard",
            GhosttyOperation.CopyTerminalTitle => "copy_title_to_clipboard",
            GhosttyOperation.Paste => "paste_from_clipboard",
            GhosttyOperation.PasteSelection => "paste_from_selection",
            _ => throw new ArgumentOutOfRangeException(nameof(operation)),
        };
        // Only fixed, developer-owned action strings enter this script.
        var script = $$"""
            tell application "Ghostty"
                if (count of windows) is 0 then
                    error "Open a Ghostty terminal window before using this action."
                end if
                set targetTerminal to focused terminal of selected tab of front window
                set didPerform to perform action "{{action}}" on targetTerminal
                if didPerform is false then
                    error "Ghostty could not perform {{action}}."
                end if
            end tell
            """;
        if (operation == GhosttyOperation.NewWindow)
            script = "tell application \"Ghostty\" to new window";
        var startInfo = new ProcessStartInfo("/usr/bin/osascript");
        startInfo.ArgumentList.Add("-e");
        startInfo.ArgumentList.Add(script);
        return startInfo;
    }
}
