namespace TotkRegistryToolkit.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FeatureAttribute(string name, string description, string registryClass, string icon, params string[] extensions) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public string RegistryClass { get; } = registryClass;
    public string[] Extensions { get; } = extensions;
    public string Icon { get; } = icon;
}
