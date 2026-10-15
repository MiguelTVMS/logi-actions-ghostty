namespace Loupedeck.GhosttyPlugin;

internal static class PluginLog
{
    private static PluginLogFile _log;

    public static void Init(PluginLogFile log) => _log = log;
    public static void Error(Exception exception, string message) => _log?.Error(exception, message);
}
