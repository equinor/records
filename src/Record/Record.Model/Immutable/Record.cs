using Records.Exceptions;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;
using Newtonsoft.Json;

namespace Records.Immutable;

[DebuggerDisplay($"{{{nameof(Id)}}}")]
public class Record : IEquatable<Record>
{
    public string Id { get; private set; } = null!;
    private IGraph _graph = new Graph();
    private readonly TripleStore _store = new();

    public List<Quad>? Provenance { get; private set; }
    public HashSet<string>? Scopes { get; private set; }
    public HashSet<string>? Describes { get; private set; }
    public List<string>? Replaces { get; private set; }
    public string? IsSubRecordOf { get; set; }

    public Record(string rdfString) => LoadFromString(rdfString);

    public Record(IGraph graph) => LoadFromGraph(graph);

    private string _nQuadsString;

    private void LoadFromString(string rdfString)
    {
        var store = new TripleStore();
        if (!string.IsNullOrEmpty(Id) || !_graph.IsEmpty || Provenance != null)
            throw new RecordException("Record is already loaded.");

        try
        {
            store.LoadFromString(rdfString);
        }
        catch
        {
            ValidateJsonLd(rdfString);
            store.LoadFromString(rdfString, new JsonLdParser());
        }
        if (store?.Graphs.Count != 1) throw new RecordException("A record must contain exactly one named graph.");
        var graph = store.Graphs.First();
        LoadFromGraph(graph);
    }
    private void LoadFromGraph(IGraph graph)
    {
        _graph = graph;
        _store.Add(_graph);

        Id = graph.Name.ToSafeString();

        Provenance = QuadsWithSubject(Id).ToList();
        if (!Provenance.Any(p => p.Object.Equals(Namespaces.Record.RecordType))) throw new RecordException("A record must have exactly one provenance object.");

        Scopes = QuadsWithPredicate(Namespaces.Record.IsInScope).Select(q => q.Object).OrderBy(s => s).ToHashSet();
        Describes = QuadsWithPredicate(Namespaces.Record.Describes).Select(q => q.Object).OrderBy(d => d).ToHashSet();

        var replaces = QuadsWithPredicate(Namespaces.Record.Replaces).Select(q => q.Object).ToArray();
        Replaces = replaces.ToList();

        var subRecordOf = QuadsWithPredicate(Namespaces.Record.IsSubRecordOf).Select(q => q.Object).ToArray();
        if (subRecordOf.Length > 1)
            throw new RecordException("A record can at most be the subrecord of one other record.");

        IsSubRecordOf = subRecordOf.FirstOrDefault();

        _nQuadsString = ToString<NQuadsWriter>();
    }

    private static void ValidateJsonLd(string rdfString)
    {
        try { JsonConvert.DeserializeObject(rdfString); }
        catch (JsonReaderException ex)
        {
            var recordException = new RecordException($"Invalid JSON-LD. See inner exception for details.", inner: ex);
            throw recordException;
        }
    }

    /// <summary>
    /// This method allows you to do a subset of SPARQL queries on your record.
    /// Supported queries: construct, select. 
    /// <example>
    /// Examples:
    /// <code>
    /// record.Sparql("select ?s where { ?s a &lt;Thing&gt; . }");
    /// </code>
    /// <code>
    /// record.Sparql("construct { ?s ?p ?o . } where { ?s ?p ?o . ?s a &lt;Thing&gt; . }");
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="queryString">String must start with "construct" or "select".</param>
    /// <exception cref="ArgumentException">
    /// Thrown if query is not "construct" or "select".
    /// </exception>
    public IEnumerable<string> Sparql(string queryString)
    {
        var command = queryString.Split().First();

        return command.ToLower() switch
        {
            "construct" => Construct(queryString),
            "select" => Select(queryString),
            _ => throw new ArgumentException("Unsupported command in SPARQL query.")
        };
    }

    public IEnumerable<Quad> Quads() => _graph.Triples.Select(t => Quad.CreateSafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<Quad> QuadsWithSubject(string subject) => QuadsWithSubject(new Uri(subject));

    public IEnumerable<Quad> QuadsWithSubject(Uri subject)
    {
        var s = _graph.CreateUriNode(subject);
        return _graph.GetTriplesWithSubject(s).Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));
    }

    public IEnumerable<Quad> QuadsWithPredicate(string predicate) => QuadsWithPredicate(new Uri(predicate));

    public IEnumerable<Quad> QuadsWithPredicate(Uri predicate)
    {
        var p = _graph.CreateUriNode(predicate);
        return _graph.GetTriplesWithPredicate(p).Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));
    }

    public IEnumerable<Quad> QuadsWithObject(string @object) => QuadsWithObject(new Uri(@object));

    public IEnumerable<Quad> QuadsWithObject(Uri @object)
    {
        var o = _graph.CreateUriNode(@object);
        return _graph.GetTriplesWithObject(o).Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));
    }

    public IEnumerable<Triple> Triples() => _graph.Triples ?? throw new UnloadedRecordException();

    public bool ContainsTriple(Triple triple) => _graph.ContainsTriple(triple);

    public bool ContainsQuad(Quad quad) => _graph.ContainsTriple(quad.ToTriple());

    public override string ToString()
    {
        return _nQuadsString;
    }

    public string ToString<T>() where T : IStoreWriter, new()
    {
        var writer = new T();
        var stringWriter = new StringWriter();
        writer.Save(_store, stringWriter);

        var result = stringWriter.ToString();

        // JsonLdWriter writes a JsonArray, but we would like only the contained JsonNode
        if (writer is JsonLdWriter) result = result[1..(result.Length - 1)];

        return result;
    }

    public bool Equals(Record? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Scopes.SetEquals(other.Scopes) && Describes.SetEquals(other.Describes);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((Record)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Scopes, Describes);
    }

    #region Private
    private IEnumerable<string> Construct(string queryString)
    {
        var results = GetResult(queryString);

        if (results is IGraph resultGraph)
        {
            return resultGraph.Triples.Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()).ToString());
        }
        else
        {
            throw new Exception("Construct failed.");
        }
    }

    private IEnumerable<string> Select(string queryString)
    {
        var results = GetResult(queryString);
        if (results is SparqlResultSet rset)
        {
            foreach (var result in rset)
            {
                yield return result.ToString();
            }
        }
        else
        {
            throw new Exception("Select failed.");
        }
    }

    private object GetResult(string queryString)
    {
        var ds = new InMemoryQuadDataset(_store, new Uri(Id ?? throw new UnloadedRecordException()));

        var processor = new LeviathanQueryProcessor(ds);
        var parser = new SparqlQueryParser();
        var query = parser.ParseFromString(queryString);

        return processor.ProcessQuery(query);
    }
    #endregion
}