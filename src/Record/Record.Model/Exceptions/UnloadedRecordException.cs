namespace Records.Exceptions;

public class UnloadedRecordException : Exception
{
    public UnloadedRecordException() : base("Initialise Record with LoadFromString.") { }

    public UnloadedRecordException(string message, Exception inner) : base(message, inner) { }
}

