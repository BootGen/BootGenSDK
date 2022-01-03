using System;

namespace BootGen;

public class NamingException : Exception {
    public string SuggestedName { get; }
    public string ActualName { get; }
    public NamingException(string message, string suggestedName, string actualName) : base(message) {
        SuggestedName = suggestedName;
        ActualName = actualName;
    }
}