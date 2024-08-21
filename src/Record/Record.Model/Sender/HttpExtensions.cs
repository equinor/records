using VDS.RDF.Writing;

namespace Records.Sender;

public static class HttpExtensions
{
    public static void AddRecord(this HttpRequestMessage message, string record) =>
        message.Content = new RecordContent(record);

    public static void AddRecord(this HttpRequestMessage message, Immutable.Record record) =>
        message.AddRecord(record.ToString<JsonLdWriter>());


    public static void AddRecordId(this HttpRequestMessage message, string recordId) =>
        message.Content = new MultipartFormDataContent() { { new StringContent(recordId), "recordId" } };

    public static void AddRecordId(this HttpRequestMessage message, Immutable.Record record) =>
        message.AddRecordId(record.Id);

}