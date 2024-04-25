using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TotkRegistryToolkit.Services;

public class ResourceService
{
    private static readonly string _path = Path.Combine(Config.DataFolder, "Assets");

    public static bool TryExtractIcon(string name, [MaybeNullWhen(false)] out string output)
    {
        string path = $"{nameof(TotkRegistryToolkit)}.Resources.{name}";
        if (Assembly.GetExecutingAssembly().GetManifestResourceStream(path) is not Stream stream) {
            output = null;
            return false;
        }

        Directory.CreateDirectory(_path);
        output = Path.Combine(_path, name);
        using FileStream fs = File.Create(output);
        stream.CopyTo(fs);
        stream.Dispose();
        return true;
    }
}
