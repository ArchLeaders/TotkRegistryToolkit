using Cocona;

namespace TotkRegistryToolkit.Components;

public class AppCommands
{
    [Command("initialize", Aliases = ["init"], Description = "Run first time initialization and setup")]
    public static void Initialize()
    {
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", $"{pathEnv};{Config.DataFolder}");

        EditCommands.Apply();
    }
}
