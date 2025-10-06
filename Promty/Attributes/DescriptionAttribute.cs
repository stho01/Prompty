namespace Promty.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property)]
public class DescriptionAttribute : Attribute
{
    public string Description { get; }
    public string? Name { get; set; }

    public DescriptionAttribute(string description)
    {
        Description = description;
    }

    public DescriptionAttribute(string name, string description)
    {
        Name = name;
        Description = description;
    }
}
