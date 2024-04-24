using Cocona;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Components;

public class ActionCommands
{
    [Command("run", Description = "Run a feature command (used internally)")]
    public static async Task Run([Argument] string feature, [Argument] string[] args)
    {
        await FeatureService.Execute(feature, args);
    }
}
