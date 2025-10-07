namespace Promty.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class FlagAliasAttribute : Attribute
{
    public string LongAlias { get; }
    public char? ShortAlias { get; }

    public FlagAliasAttribute(string longAlias, char shortAlias)
    {
        LongAlias = longAlias;
        ShortAlias = shortAlias;
    }

    public FlagAliasAttribute(string longAlias)
    {
        LongAlias = longAlias;
        ShortAlias = null;
    }

    public FlagAliasAttribute(char shortAlias)
    {
        LongAlias = string.Empty;
        ShortAlias = shortAlias;
    }
}
