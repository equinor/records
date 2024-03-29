﻿using Records.Exceptions;
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
    private InMemoryDataset _dataset;
    private LeviathanQueryProcessor _queryProcessor;

    public List<Quad>? Provenance { get; private set; }
    public HashSet<string>? Scopes { get; private set; }
    public HashSet<string>? Describes { get; private set; }
    public List<string>? Replaces { get; private set; }

    public string? IsSubRecordOf { get; set; }

    public Record(string rdfString) => LoadFromString(rdfString);
    public Record(string rdfString, IStoreReader reader) => LoadFromString(rdfString, reader);
    public Record(IGraph graph) => LoadFromGraph(graph);
    public Record(ITripleStore store) => LoadFromTripleStore(store);

    private string _nQuadsString;

    private void LoadFromString(string rdfString, IStoreReader reader)
    {
        var store = new TripleStore();
        if (!string.IsNullOrEmpty(Id) || !_graph.IsEmpty || Provenance != null)
            throw new RecordException("Record is already loaded.");

        store.LoadFromString(rdfString, reader);

        LoadFromTripleStore(store);
    }

    private void LoadFromTripleStore(ITripleStore store)
    {
        if (store?.Graphs.Count != 1) throw new RecordException("A record must contain exactly one named graph.");
        var graph = store.Graphs.First();
        LoadFromGraph(graph);
    }

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

        LoadFromTripleStore(store);
    }

    private void LoadFromGraph(IGraph graph)
    {
        _graph = graph;
        _store.Add(_graph);
        _dataset = new InMemoryDataset(graph);
        _queryProcessor = new LeviathanQueryProcessor(_dataset);

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

    public IGraph Graph()
    {
        var tempGraph = new Graph();
        tempGraph.Merge(_graph);
        return tempGraph;
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
        var parser = new SparqlQueryParser();
        var query = parser.ParseFromString(queryString);

        return command.ToLower() switch
        {
            "construct" => Construct(query),
            "select" => Select(query),
            _ => throw new ArgumentException("Unsupported command in SPARQL query.")
        };
    }

    /// <summary>
    /// This method allows you to do select and ask SPARQL queries on your record.
    /// See DotNetRdf documentation on how to create SparqlQuery and parse results.
    /// <example>
    /// Examples:
    /// <code>
    /// record.query(new SparqlQueryParser().ParseFromString("select ?s where { ?s a &lt;Thing&gt; . }"));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="query">Query to be evaluated over the triples in the record graph</param>
    /// <exception cref="ArgumentException">
    /// Thrown if query is not "ask" or "select".
    /// </exception>
    public SparqlResultSet Query(SparqlQuery query) =>
        _queryProcessor.ProcessQuery(query) switch
        {
            SparqlResultSet res => res,
            _ => throw new ArgumentException(
                "DotNetRdf did not return SparqlResultSet. Probably the Sparql query was not a select or ask query")
        };


    /// <summary>
    /// This method allows you to do select and ask SPARQL queries on your record.
    /// See DotNetRdf documentation on how to create SparqlQuery and parse results.
    /// <example>
    /// Examples:
    /// <code>
    /// record.Sparql(new SparqlQueryParser().ParseFromString("construct { ?s ?p ?o . } where { ?s ?p ?o . ?s a &lt;Thing&gt; . }"));
    /// </code>
    /// </example>
    /// </summary>
    /// <param name="query">Query to be evaluated over the triples in the record graph</param>
    /// <exception cref="ArgumentException">
    /// Thrown if query is not "construct".
    /// </exception>
    public IGraph ConstructQuery(SparqlQuery query) =>
        _queryProcessor.ProcessQuery(query) switch
        {
            IGraph res => res,
            _ => throw new ArgumentException(
                "DotNetRdf did not return IGraph. Probably the Sparql query was not a construct query")
        };

    public IEnumerable<Quad> Quads() => _graph.Triples.Select(t => Quad.CreateSafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<INode> SubjectWithType(string type) => SubjectWithType(new Uri(type));
    public IEnumerable<INode> SubjectWithType(Uri type) => SubjectWithType(new UriNode(type));
    public IEnumerable<INode> SubjectWithType(INode type)
        => _graph
        .GetTriplesWithPredicateObject(new UriNode(new Uri(Namespaces.Rdf.Type)), type)
        .Select(t => t.Subject);

    public IEnumerable<string> LabelsOfSubject(string subject) => LabelsOfSubject(new Uri(subject));
    public IEnumerable<string> LabelsOfSubject(Uri subject) => LabelsOfSubject(new UriNode(subject));
    public IEnumerable<string> LabelsOfSubject(INode subject)
        => _graph
        .GetTriplesWithSubjectPredicate(subject, new UriNode(new Uri(Namespaces.Rdfs.Label)))
        .Where(t => t.Object is LiteralNode literal)
        .Select(t => ((LiteralNode)t.Object).Value);


    public IEnumerable<Quad> QuadsWithSubject(string subject) => QuadsWithSubject(new Uri(subject));
    public IEnumerable<Quad> QuadsWithSubject(Uri subject) => QuadsWithSubject(new UriNode(subject));
    public IEnumerable<Quad> QuadsWithSubject(INode subject)
        => _graph
        .GetTriplesWithSubject(subject)
        .Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<Quad> QuadsWithPredicate(string predicate) => QuadsWithPredicate(new Uri(predicate));
    public IEnumerable<Quad> QuadsWithPredicate(Uri predicate) => QuadsWithPredicate(new UriNode(predicate));
    public IEnumerable<Quad> QuadsWithPredicate(INode predicate)
      => _graph
        .GetTriplesWithPredicate(predicate)
        .Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<Quad> QuadsWithObject(string @object) => QuadsWithObject(new Uri(@object));
    public IEnumerable<Quad> QuadsWithObject(Uri @object) => QuadsWithObject(new UriNode(@object));
    public IEnumerable<Quad> QuadsWithObject(INode @object)
        => _graph
        .GetTriplesWithObject(@object)
        .Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<Quad> QuadsWithSubjectPredicate(string subject, string predicate) => QuadsWithSubjectPredicate(new Uri(subject), new Uri(predicate));
    public IEnumerable<Quad> QuadsWithSubjectPredicate(Uri subject, Uri predicate) => QuadsWithSubjectPredicate(new UriNode(subject), new UriNode(predicate));
    public IEnumerable<Quad> QuadsWithSubjectPredicate(INode subject, INode predicate)
        => _graph
        .GetTriplesWithSubjectPredicate(subject, predicate)
        .Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<Quad> QuadsWithPredicateAndObject(string predicate, string @object) => QuadsWithPredicateAndObject(new Uri(predicate), new Uri(@object));
    public IEnumerable<Quad> QuadsWithPredicateAndObject(Uri predicate, Uri @object) => QuadsWithPredicateAndObject(new UriNode(predicate), new UriNode(@object));
    public IEnumerable<Quad> QuadsWithPredicateAndObject(INode predicate, INode @object)
        => _graph
        .GetTriplesWithPredicateObject(predicate, @object)
        .Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));

    public IEnumerable<Quad> QuadsWithSubjectObject(string subject, string @object) => QuadsWithSubjectObject(new Uri(subject), new Uri(@object));
    public IEnumerable<Quad> QuadsWithSubjectObject(Uri subject, Uri @object) => QuadsWithSubjectObject(new UriNode(subject), new UriNode(@object));
    public IEnumerable<Quad> QuadsWithSubjectObject(UriNode subject, UriNode @object)
        => _graph
        .GetTriplesWithSubjectObject(subject, @object)
        .Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()));


    public IEnumerable<Triple> Triples() => _graph.Triples ?? throw new UnloadedRecordException();
    public IEnumerable<Triple> ContentAsTriples()
    {
        var parser = new SparqlQueryParser();
        var provenance = ProvenanceAsTriples();

        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT {?s ?p ?o} WHERE { GRAPH @Id { ?s ?p ?o FILTER(?s != @Id)} }");
        parameterizedQuery.SetUri("Id", new Uri(Id));
        var contentQueryString = parameterizedQuery.ToString();
        var contentQuery = parser.ParseFromString(contentQueryString);
        var content = ConstructQuery(contentQuery);
        return content.Triples.Except(provenance);
    }

    public IEnumerable<Triple> ProvenanceAsTriples()
    {
        var parser = new SparqlQueryParser();
        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT {@Id ?p ?o} WHERE { GRAPH @Id { @Id ?p ?o} }");
        parameterizedQuery.SetUri("Id", new Uri(Id));
        var provenanceQueryString = parameterizedQuery.ToString();

        var provenanceQuery = parser.ParseFromString(provenanceQueryString);
        var provenance = ConstructQuery(provenanceQuery);
        return provenance.Triples;
    }

    public bool ContainsTriple(Triple triple) => _graph.ContainsTriple(triple);

    public bool ContainsQuad(Quad quad) => _graph.ContainsTriple(quad.ToTriple());

    public override string ToString()
    {
        return _nQuadsString;
    }

    public string ToString<T>() where T : IStoreWriter, new()
        => ToString(new T());

    public string ToString(IStoreWriter writer)
    {
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
    private IEnumerable<string> Construct(SparqlQuery query)
    {
        var resultGraph = ConstructQuery(query);
        return resultGraph.Triples.Select(t => Quad.CreateUnsafe(t, Id ?? throw new UnloadedRecordException()).ToString());
    }

    private IEnumerable<string> Select(SparqlQuery query)
    {
        var rset = Query(query);
        foreach (var result in rset)
        {
            yield return result.ToString();
        }
    }


    #endregion

}