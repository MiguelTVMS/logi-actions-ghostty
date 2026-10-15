namespace Loupedeck.GhosttyPlugin.Platform;

public static class GhosttyControllerFactory
{
    public static IGhosttyController Create() => Create(OperatingSystem.IsMacOS(), OperatingSystem.IsWindows());

    internal static IGhosttyController Create(bool isMacOs, bool isWindows)
    {
        if (isMacOs) return new GhosttyControllerMacOs();
        if (isWindows) return new GhosttyControllerWindows();
        throw new PlatformNotSupportedException("The Logitech Ghostty plugin currently supports macOS only.");
    }
}
