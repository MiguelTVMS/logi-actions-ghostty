namespace Loupedeck.GhosttyPlugin;

using Loupedeck.GhosttyPlugin.Platform;

public abstract class GhosttyCommandCommon : PluginDynamicCommand
{
    private readonly Func<IGhosttyController> _createController;
    private readonly Action<Exception, string> _logError;

    protected GhosttyCommandCommon(string name, string description, string group,
        Func<IGhosttyController> createController = null, Action<Exception, string> logError = null)
        : base(name, description, group)
    {
        _createController = createController ?? GhosttyControllerFactory.Create;
        _logError = logError ?? PluginLog.Error;
    }

    protected override void RunCommand(string actionParameter) => _ = ExecuteSafelyAsync();

    internal async Task ExecuteSafelyAsync()
    {
        try
        {
            await ExecuteAsync(_createController());
        }
        catch (Exception exception)
        {
            _logError(exception, "Ghostty action failed.");
        }
    }

    protected abstract Task ExecuteAsync(IGhosttyController controller);
}
