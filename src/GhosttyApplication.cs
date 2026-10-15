namespace Loupedeck.GhosttyPlugin;

// Associates the plugin and its default profile with the native Ghostty application.
public sealed class GhosttyApplication : ClientApplication
{
    protected override string GetProcessName() => "";
    protected override string GetBundleName() => "com.mitchellh.ghostty";
    public override ClientApplicationStatus GetApplicationStatus() => ClientApplicationStatus.Unknown;
}
