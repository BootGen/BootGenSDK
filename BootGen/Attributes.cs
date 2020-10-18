using System;

public class ClientOnlyAttribute : Attribute
{
}

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

public class GetAttribute : Attribute
{
}

public class PostAttribute : Attribute
{
}

public class PutAttribute : Attribute
{
}

public class PatchAttribute : Attribute
{
}

public class DeleteAttribute : Attribute
{
}
