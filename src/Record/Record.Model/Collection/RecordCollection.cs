using VDS.RDF;
using Record = Records.Immutable.Record;

namespace Records.Collection;

public class RecordCollection
{
    internal TripleStore _store = new();
    public IEnumerable<Record> Records { get; internal set; } = [];

    public RecordCollection(string recordStrings, IStoreReader parser)
    {
        var tempStore = new TripleStore();
        tempStore.LoadFromString(recordStrings, parser);
        var records = tempStore.FindRecords();

        LoadRecordsIntoTripleStore(records);
    }

    public RecordCollection(IEnumerable<string> recordStrings, IStoreReader parser)
    {
        var tempStore = new TripleStore();
        foreach (var recordString in recordStrings)
        {
            tempStore.LoadFromString(recordString, parser);
        }

        var records = tempStore.FindRecords();
        
        LoadRecordsIntoTripleStore(records);
    }

    public RecordCollection(IEnumerable<Record> records) : this(records.ToArray()) { }

    public RecordCollection(params Record[] records) => LoadRecordsIntoTripleStore(records);

    private void LoadRecordsIntoTripleStore(IEnumerable<Record> records)
    {
        foreach (var record in records)
            foreach (var graph in record.TripleStore().Graphs)
                if (!graph.IsEmpty && graph.Name != null)
                    _store.Add(graph);

        var foundRecords = _store.FindRecords();

        Records = foundRecords.ToArray();
    }
}