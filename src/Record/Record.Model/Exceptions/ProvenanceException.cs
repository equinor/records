namespace Records.Exceptions;

public class ProvenanceException : Exception
{
    private const string _messageTemplate = "Error in parsing of provenance in record.{0}";
    public ProvenanceException(string message) : base(string.Format(_messageTemplate, $"\n{message}")) { }
}