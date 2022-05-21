using System;

namespace BootGen;

public class NamingException : Exception {
    public string SuggestedName { get; }
    public string ActualName { get; }
    public bool IsArray { get; }
    public NamingException(string message, string suggestedName, string actualName, bool isArray) : base(message)
    {
        SuggestedName = suggestedName;
        ActualName = actualName;
        IsArray = isArray;
    }
}