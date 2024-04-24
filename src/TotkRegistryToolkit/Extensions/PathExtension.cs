namespace TotkRegistryToolkit.Extensions;

public static class PathExtension
{
    public static string TrimExtension(this string path)
    {
        ReadOnlySpan<char> pathChars = path;
        int lastDot = pathChars.LastIndexOf('.');
        return pathChars[..lastDot].ToString();
    }
}
