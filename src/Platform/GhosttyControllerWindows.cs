namespace Loupedeck.GhosttyPlugin.Platform;

using System.Diagnostics;

// Deliberate boundary for future official Ghostty Windows support.
public sealed class GhosttyControllerWindows : GhosttyControllerCommon
{
    protected override ProcessStartInfo CreateStartInfo(GhosttyOperation operation) =>
        throw new PlatformNotSupportedException("Ghostty Windows support is pending an official release and native validation.");
}
