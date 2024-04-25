using System.Text.Json;
using System.Text.Json.Serialization;

namespace TotkRegistryToolkit.Models;

public class TotkConfig
{
    private static readonly string _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "totk", "Config.json");

    public static TotkConfig Shared { get; } = Load();

    public string GamePath { get; set; } = string.Empty;

    [JsonIgnore]
    public string ZsDicPath => Path.Combine(GamePath, "Pack", "ZsDic.pack.zs");

    public static TotkConfig Load()
    {
        if (!File.Exists(_path)) {
            return new();
        }

        using FileStream fs = File.OpenRead(_path);
        return JsonSerializer.Deserialize<TotkConfig>(fs)
            ?? new();
    }

    public void Save()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        using FileStream fs = File.Create(_path);
        JsonSerializer.Serialize(fs, this);
    }
}
