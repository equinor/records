namespace Records.Exceptions;

public class ProvenanceException(string message) : Exception(string.Format(_messageTemplate, $"\n{message}"))
{
    private const string _messageTemplate = "Error in parsing of provenance in record.{0}";
}