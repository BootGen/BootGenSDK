using System;
public class ServerOnlyAttribute : Attribute
{
}

public class HasTimestampsAttribute : Attribute
{
}

public class AuthenticateAttribute : Attribute
{
}

public class ReadonlyAttribute : Attribute
{
}

public class GenerateAttribute : Attribute
{
    public GenerateAttribute(bool controller, bool serviceInterface, bool service)
    {
    }
}
public class ControllerNameAttribute : Attribute
{
    public ControllerNameAttribute(string value)
    {
    }
}

public class ServiceNameAttribute : Attribute
{
    public ServiceNameAttribute(string value)
    {
    }
}

public class PluralNameAttribute : Attribute
{
    public PluralNameAttribute(string value)
    {
    }
}
public class SingularNameAttribute : Attribute
{
    public SingularNameAttribute(string value)
    {
    }
}

public class OneToManyAttribute : Attribute
{
    public OneToManyAttribute(string parentName = null)
    {
    }
}

public class ManyToManyAttribute : Attribute
{
    public ManyToManyAttribute(string pivotName)
    {
    }
}

