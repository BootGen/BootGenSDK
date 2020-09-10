using System;

public class ResourceAttribute : Attribute
{
}

public class ClientOnlyAttribute : Attribute
{
}

public class ServerOnlyAttribute : Attribute
{
}

public class HasTimestampsAttribute : Attribute
{
}

public class PluralName : Attribute
{
    public string Value { get; }
    public PluralName(string value)
    {
        Value = value;
    }
}
