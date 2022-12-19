namespace Records.Exceptions;
public class QuadException : Exception
{
    private static string _messageTemplate => "Failure in quad. {0}";

    public QuadException() : base(string.Format(_messageTemplate, string.Empty)) { }
    public QuadException(string message) : base(string.Format(_messageTemplate, message)) { }
    public QuadException(string message, Exception inner) : base(string.Format(_messageTemplate, message), inner) { }
}