using VDS.RDF.Writing;
using Microsoft.Azure.Functions.Worker.Http;

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

    public static Immutable.Record ToRecord(this HttpRequestData message)
    {
        var recordString = message.ReadAsString();
        if (string.IsNullOrEmpty(recordString))
            throw new ArgumentException("message.ReadAsString() returned null or empty.");

        return new Immutable.Record(recordString);
    }
}