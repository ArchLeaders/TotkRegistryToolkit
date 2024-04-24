using Cocona;
using System.Text.Json;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Components;

public class EditCommands
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(TotkRegistryToolkit), "Metadata.json");

    [Command("apply", Aliases = ["refresh"], Description = "Apply all features to the registry")]
    public static void Apply()
    {
        Dictionary<string, bool> metadata = GetMetadata();
        // TODO: Apply to registry
    }

    [Command("reset", Description = "Reset all features to the default state")]
    public static void Reset()
    {
        SaveMetadata([]);
        Apply();
    }

    [Command("feature", Aliases = ["feat"], Description = "Get or set a registry feature")]
    public static void Feature([Argument] string name, [Option(Description = "Enable the feature", StopParsingOptions = true)] bool enable, [Option(Description = "Disable the feature", StopParsingOptions = true)] bool disable)
    {
        Dictionary<string, bool> metadata = GetMetadata();

        bool state = true;

        if (enable) {
            metadata[name] = true;
        }

        if (disable) {
            metadata[name] = false;
        }

        Console.WriteLine($"{name}: {GetStateDescription(state)}");

        SaveMetadata(metadata);

        // TODO: Apply to registry
    }

    [Command("list", Aliases = ["ls"], Description = "List every feature")]
    public static void List()
    {
        Console.WriteLine("""
            Features:

            """);

        Dictionary<string, bool> metadata = GetMetadata();

        foreach (var (name, description) in FeatureService.Features) {
            bool state = metadata.GetValueOrDefault(name, true);
            Console.WriteLine($"- {name}: {description} [{GetStateDescription(state)}]");
        }
    }

    private static Dictionary<string, bool> GetMetadata()
    {
        if (File.Exists(_path)) {
            using FileStream fs = File.OpenRead(_path);
            return JsonSerializer.Deserialize<Dictionary<string, bool>>(fs) ?? [];
        }
        else {
            return [];
        }
    }

    private static void SaveMetadata(Dictionary<string, bool> metadata)
    {
        if (Path.GetDirectoryName(_path) is string folder) {
            Directory.CreateDirectory(folder);
        }

        using FileStream output = File.Create(_path);
        JsonSerializer.Serialize(output, metadata);
    }

    private static string GetStateDescription(bool state)
    {
        return state switch {
            true => "Enabled",
            false => "Disabled"
        };
    }
}
