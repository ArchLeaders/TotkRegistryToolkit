namespace TotkRegistryToolkit;

public class Config
{
    public static readonly string DataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), nameof(TotkRegistryToolkit));
}
