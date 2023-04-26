namespace Records.Exceptions;

public class FileRecordException : Exception
{
    private static string _messageTemplate => "Failure in building file content. {0}";

    public FileRecordException() : base(string.Format(_messageTemplate, string.Empty)) { }
    public FileRecordException(string message) : base(string.Format(_messageTemplate, message)) { }
    public FileRecordException(string message, Exception inner) : base(string.Format(_messageTemplate, message), inner) { }
}