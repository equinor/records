using Records.Sender;
using static Records.Sender.RecordMessageBuilder;

namespace Records.Immutable;

public static class RecordExtensions
{
    public static RecordMessageBuilder CreateRecordMessageBuilder(this Record record, string token) => new RecordMessageBuilder(token).WithRecord(record);

    public static HttpRequestMessage CreateHttpRequestMessage(this Record record, string token, string endpoint, int cursor, RecordOperation operation)
        => new RecordMessageBuilder(token).WithCursor(cursor).ForEndpoint(endpoint).WithRecord(record).Build(operation);
}
