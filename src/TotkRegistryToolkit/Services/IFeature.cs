namespace TotkRegistryToolkit.Services;

public interface IFeature
{
    public static abstract ValueTask Execute(string[] args);
}
