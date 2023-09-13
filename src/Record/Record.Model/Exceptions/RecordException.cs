namespace Records.Exceptions;

public class RecordException : Exception
{
    public RecordException() : base() { }
    public RecordException(string message) : base(message) { }
    public RecordException(string message, Exception inner) : base(message, inner) { }
}