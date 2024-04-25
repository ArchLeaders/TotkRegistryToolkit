using Cocona;
using TotkRegistryToolkit.Models;

namespace TotkRegistryToolkit.Components;

[HasSubCommands(typeof(ConfigCommands), "config", Description = "Configuration commands")]
public class AppCommands
{
    [Command("initialize", Aliases = ["init"], Description = "Run first time initialization and setup")]
    public static void Initialize()
    {
        string? pathEnv = Environment.GetEnvironmentVariable("PATH");
        Environment.SetEnvironmentVariable("PATH", $"{pathEnv};{Config.DataFolder}", EnvironmentVariableTarget.Machine);

        FeatureCommands.Apply();
    }

    public class ConfigCommands
    {
        [Command("set-game-path")]
        public static void SetGamePath([Argument] string gamePath)
        {
            TotkConfig.Shared.GamePath = gamePath;
            TotkConfig.Shared.Save();
        }
    }
}
