namespace Records.Exceptions;

public class FileException : Exception
{
    private static string _messageTemplate => "Failure in building file content. {0}";

    public FileException() : base(string.Format(_messageTemplate, string.Empty)) { }
    public FileException(string message) : base(string.Format(_messageTemplate, message)) { }
    public FileException(string message, Exception inner) : base(string.Format(_messageTemplate, message), inner) { }
}