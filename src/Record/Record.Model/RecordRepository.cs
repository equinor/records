using System.Collections;
using System.Runtime.CompilerServices;
using AngleSharp.Common;
using HtmlAgilityPack;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Writing;
using Record = Records.Immutable.Record;

namespace Records;

public sealed class RecordRepository : RecordRepository<NQuadsRecordWriter>
{
    public RecordRepository() : base() { }

    public RecordRepository(Record record) : base(record) { }

    public RecordRepository(IDictionary<string, Record> records) : base(records) { }

    public RecordRepository(IEnumerable<Record> records) : base(records) { }
}

public class RecordRepository<T> : IEnumerable<Record> where T : IRdfWriter, new()
{
    public TripleStore _store = new();
    public int Count => _store.Graphs.Count;

    public RecordRepository() { }

    public RecordRepository(Record record) => this.Add(record);

    public RecordRepository(IDictionary<string, Record> records) => this.Add(records);

    public RecordRepository(IEnumerable<Record> records) => this.Add(records);

    public void Add(string id, Record record)
    {
        if (record?.Id == null) throw new UnloadedRecordException();
        if (id != record.Id) throw new ArgumentException("Record ID does not match given ID.");

        _store.AddRecord(record);
    }

    public void Add(Record record) => this.Add(record.Id ?? throw new UnloadedRecordException(), record);

    public void Add(IEnumerable<Record> records) { foreach (var record in records) this.Add(record); }

    public void Add(IDictionary<string, Record> records) { foreach (var (id, record) in records) this.Add(id, record); }

    public bool TryGetRecord(string id, out Record outRecord) => _store.TryGetRecord(id, out outRecord);

    public bool TryGetRecord(Record record, out Record outRecord) => TryGetRecord(record.Id ?? throw new UnloadedRecordException(), out outRecord);

    public bool TryRemove(Uri id)
    {
        if (!_store.HasGraph(id)) return false;
        _store.Remove(id);
        return true;
    }

    public bool TryRemove(string id) => TryRemove(new Uri(id));

    public bool TryRemove(Record record) => TryRemove(new Uri(record.Id) ?? throw new UnloadedRecordException());

    public bool Contains(Uri id) => _store.HasGraph(id);

    public bool Contains(string id) => Contains(new Uri(id));

    public RepositoryValidity Validate()
    {
        var sameRecords = new Dictionary<Record, List<Record>>();

        var recordsArray = _store.Records().ToArray();
        for (var i = 0; i < recordsArray.Length - 1; i++)
        {
            var left = recordsArray[i];
            for (var j = i + 1; j < recordsArray.Length; j++)
            {
                var right = recordsArray[j];

                if (!left.Equals(right)) continue;
                if (left.Replaces.Contains(right.Id) || right.Replaces.Contains(left.Id)) continue;

                if (!sameRecords.ContainsKey(left)) sameRecords[left] = new();
                sameRecords[left].Add(right);
            }
        }

        if (sameRecords.Count == 0) return new(true, null);
        return new(false, sameRecords);
    }

    public record RepositoryValidity(bool Valid, IDictionary<Record, List<Record>>? ProblemRecords);

    public IEnumerator<Record> GetEnumerator() => _store.Records().GetEnumerator();

    public override string ToString()
    {
        var r = "";
        foreach (var graph in _store.Graphs)
        {
            var writer = new T();
            var stringWriter = new System.IO.StringWriter();
            writer.Save(graph, stringWriter);
            r += stringWriter.ToString();
        }
        return r;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public static class RecordStoreExtensions
{
    public static void AddRecord(this TripleStore store, Record record)
    {
        store.LoadFromString(record.ToString<NQuadsWriter>());
    }

    public static IEnumerable<Record> Records(this TripleStore store)
    {
        foreach (var graph in store.Graphs) yield return new(graph.Stringify());
    }

    public static bool TryGetRecord(this TripleStore store, string id, out Record? record)
    {
        record = default(Record);
        if (!store.HasGraph(new Uri(id))) return false;

        var graph = store.Graphs[new Uri(id)];

        record = new Record(graph.Stringify());
        return true;
    }

    public static string Stringify(this IGraph graph)
    {
        var writer = new NQuadsRecordWriter();
        var sw = new System.IO.StringWriter();
        writer.Save(graph, sw);
        return sw.ToString();
    }
}