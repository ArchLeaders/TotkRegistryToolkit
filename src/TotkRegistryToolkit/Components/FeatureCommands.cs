using Cocona;
using System.Text.Json;
using TotkRegistryToolkit.Attributes;
using TotkRegistryToolkit.Services;
using TotkRegistryToolkit.Win32;

namespace TotkRegistryToolkit.Components;

public class FeatureCommands
{
    private static readonly string _path = Path.Combine(Config.DataFolder, "Metadata.json");

    [Command("apply", Aliases = ["refresh"], Description = "Apply all features to the registry")]
    public static void Apply()
    {
        Dictionary<string, bool> metadata = GetMetadata();
        foreach ((string name, FeatureAttribute feature) in FeatureService.Features) {
            switch (metadata.GetValueOrDefault(name, true)) {
                case true:
                    RegistryInterop.Create(feature);
                    break;
                case false:
                    RegistryInterop.Delete(feature);
                    break;
            }
        }
    }

    [Command("reset", Description = "Reset all features to the default state")]
    public static void Reset()
    {
        SaveMetadata([]);
        Apply();
    }

    [Command("update", Description = "Enable or disable a feature")]
    public static void Update([Argument] string feature, [Option(Description = "Enable the feature", StopParsingOptions = true)] bool enable, [Option(Description = "Disable the feature", StopParsingOptions = true)] bool disable)
    {
        Dictionary<string, bool> metadata = GetMetadata();

        bool state = true;

        if (enable) {
            state = metadata[feature] = true;
            RegistryInterop.Create(FeatureService.Features[feature]);
        }

        if (disable) {
            state = metadata[feature] = false;
            RegistryInterop.Delete(FeatureService.Features[feature]);
        }

        SaveMetadata(metadata);

        Console.WriteLine($"{feature}: {GetStateDescription(state)}");
    }

    [Command("list", Aliases = ["ls"], Description = "List every feature")]
    public static void List()
    {
        Console.WriteLine("""
            Features:

            """);

        Dictionary<string, bool> metadata = GetMetadata();

        foreach ((_, FeatureAttribute feature) in FeatureService.Features) {
            bool state = metadata.GetValueOrDefault(feature.Name, true);
            Console.WriteLine($"- {feature.Name}: {feature.Description} [{GetStateDescription(state)}]");
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
