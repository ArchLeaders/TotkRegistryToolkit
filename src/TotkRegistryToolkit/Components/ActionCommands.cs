using Cocona;
using TotkRegistryToolkit.Services;
using TotkRegistryToolkit.Win32;

namespace TotkRegistryToolkit.Components;

public class ActionCommands
{
    [Command("run", Description = "Run a feature command (used internally)")]
    public static async Task Run([Argument] string feature, [Argument] string[] args)
    {
        ConsoleInterop.SetWindowMode(WindowMode.Hidden);

        try {
            await FeatureService.Execute(feature, args);
        }
        catch (Exception ex) {
            ConsoleInterop.SetWindowMode(WindowMode.Visible);
            Console.WriteLine(ex);
            Console.ReadLine();
        }
    }
}
