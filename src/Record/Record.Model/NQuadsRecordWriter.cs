using VDS.RDF;
using VDS.RDF.Writing;

namespace Records;

public class NQuadsRecordWriter : RecordStoreWriter<NQuadsWriter> { }

public class JsonLdRecordWriter : RecordStoreWriter<JsonLdWriter> { }

public class RecordStoreWriter<T> : IRdfWriter where T : IStoreWriter, new()
{
    private readonly TripleStore _store;

    public RecordStoreWriter()
    {
        _store = new TripleStore();
    }

    public void Save(IGraph g, string filename)
    {
        throw new NotImplementedException();
    }

    public void Save(IGraph g, TextWriter output)
    {
        _store.Add(g);
        var writer = new T();
        writer.Warning += (message) => Warning?.Invoke(message);
        writer.Save(_store, output);
    }

    public void Save(IGraph g, TextWriter output, bool leaveOpen)
    {
        throw new NotImplementedException();
    }

    public event RdfWriterWarning? Warning;
}