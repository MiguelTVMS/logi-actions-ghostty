namespace Loupedeck.GhosttyPlugin;

public class GhosttyPlugin : Plugin
{
    public override bool UsesApplicationApiOnly => true;
    public override bool HasNoApplication => false;

    public override void Load() => PluginLog.Init(this.Log);
    public override void Unload() => PluginLog.Init(null);
}
