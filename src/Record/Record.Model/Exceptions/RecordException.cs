namespace Records.Exceptions;

public class RecordException : Exception
{
    private static string _messageTemplate => "Failure in record. {0}";

    public RecordException() : base(string.Format(_messageTemplate, string.Empty)) { }
    public RecordException(string message) : base(string.Format(_messageTemplate, message)) { }
    public RecordException(string message, Exception inner) : base(string.Format(_messageTemplate, message), inner) { }
}