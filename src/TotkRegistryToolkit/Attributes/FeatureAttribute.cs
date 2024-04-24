namespace TotkRegistryToolkit.Attributes;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public class FeatureAttribute(string name, string description) : Attribute
{
    public string Name { get; } = name;
    public string Description { get; } = description;
}
