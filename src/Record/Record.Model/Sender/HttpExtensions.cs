using VDS.RDF.Writing;
using Microsoft.Azure.Functions.Worker.Http;
using Records.Backend;

namespace Records.Sender;

public static class HttpExtensions
{
    public static void AddRecord(this HttpRequestMessage message, string record) =>
        message.Content = new RecordContent(record);

    public static async Task AddRecord(this HttpRequestMessage message, Immutable.Record record) =>
        message.AddRecord(await record.ToString<JsonLdWriter>());


    public static Task AddRecordId(this HttpRequestMessage message, string recordId)
    {
        message.Content = new MultipartFormDataContent() { { new StringContent(recordId), "recordId" } };
        return Task.CompletedTask;
    }

    public static void AddRecordId(this HttpRequestMessage message, Immutable.Record record) =>
        message.AddRecordId(record.Id);

    // Assumes the message contains a valid Record on any RDF format handled by DotNetRdf
    public static async Task<Immutable.Record> ToRecord(this HttpRequestData message)
    {
        var recordString = message.ReadAsString();
        if (string.IsNullOrEmpty(recordString))
            throw new ArgumentException("message.ReadAsString() returned null or empty.");

        return await Immutable.Record.CreateAsync(new DotNetRdfRecordBackend(recordString));
    }
}