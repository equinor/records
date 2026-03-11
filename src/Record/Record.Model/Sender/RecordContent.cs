using VDS.RDF.Writing;

namespace Records.Sender;

public class RecordContent : StringContent
{
    public RecordContent(string record) : base(record)
    {
        Headers.ContentType = new("application/ld+json");
    }

    public static async Task<RecordContent> CreateAsync(Immutable.Record record) =>
        new(await record.ToString<JsonLdWriter>());

}