using System.Text;
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
        var sw = new System.IO.StringWriter();
        writer.Save(_store, sw);
        var rdf = sw.ToString();

        if(writer is JsonLdRecordWriter)
        {
            output.Write(rdf);
            return;
        }

        if(rdf.Split("\r\n").First().Split(" ").Count() == 5)
        {
            output.Write(rdf);
            return;
        }

        var result = string.Empty;
        foreach (var line in rdf.Split("\n"))
        {
            if (string.IsNullOrEmpty(line))
                break;
            result += line.Split(" .")[0] + $" <{g.BaseUri}> .\r\n";
        }
        output.Write(result);
    }

    public void Save(IGraph g, TextWriter output, bool leaveOpen)
    {
        throw new NotImplementedException();
    }

    public void Save(IGraph g, string filename, Encoding fileEncoding)
    {
        throw new NotImplementedException();
    }

    public event RdfWriterWarning? Warning;
}