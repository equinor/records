namespace Records.Exceptions;

public class RecordException : Exception
{
    public string? _rdfstring { get; init; }
    public RecordException() : base() { }
    public RecordException(string message) : base(message) { }
    public RecordException(string message, Exception inner) : base(message, inner) { }

    public RecordException(string message, string rdfstring) : base(message)
    {
        _rdfstring = rdfstring;
    }
}