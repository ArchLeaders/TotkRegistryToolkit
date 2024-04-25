using Microsoft.Win32;
using System.Data;
using TotkRegistryToolkit.Attributes;
using TotkRegistryToolkit.Services;

namespace TotkRegistryToolkit.Win32;

public class RegistryInterop
{
    private static readonly string _exe = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(TotkRegistryToolkit), "tkrt.exe");
    private static readonly string _command = $$"""
        "{{_exe}}" run "{0}" "%1"
        """;

    public static void Create(FeatureAttribute feature)
    {
        using RegistryKey shell = GetShell(feature);

        string name = GetRegistryName(feature.Name);
        using RegistryKey key = shell.CreateSubKey(name);
        key.SetValue(string.Empty, feature.Description);

        if (feature.Extensions.Length > 0) {
            key.SetValue("AppliesTo",
                string.Join(" OR ", feature.Extensions.Select(x => x.StartsWith('.') ? $"System.FileExtension:={x}" : $"System.FileExtension:({x})"))
            );
        }

        if (ResourceService.TryExtractIcon(feature.Icon, out string? path)) {
            key.SetValue("Icon", path);
        }

        using RegistryKey commandKey = key.CreateSubKey("command");
        commandKey.SetValue(string.Empty, string.Format(_command, feature.Name));
    }

    public static void Delete(FeatureAttribute feature)
    {
        using RegistryKey shell = GetShell(feature);

        string name = GetRegistryName(feature.Name);
        if (shell.GetSubKeyNames().Contains(name)) {
            shell.DeleteSubKeyTree(name);
        }
    }

    private static RegistryKey GetShell(FeatureAttribute feature)
    {
        using RegistryKey root = Registry.ClassesRoot
            .CreateSubKey($"{feature.RegistryClass}\\shell\\totk");

        root.SetValue("MUIVerb", "TotK", RegistryValueKind.String);
        root.SetValue("SubCommands", string.Empty, RegistryValueKind.String);

        return root.CreateSubKey("shell");
    }

    private static string GetRegistryName(string name)
    {
        return name
            .ToLower()
            .Replace(' ', '_');
    }
}
