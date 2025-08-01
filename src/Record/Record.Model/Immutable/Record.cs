﻿using Newtonsoft.Json;
using Records.Exceptions;
using System.Diagnostics;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;

namespace Records.Immutable;

[DebuggerDisplay($"{{{nameof(Id)}}}")]
public class Record : IEquatable<Record>
{
    public string Id { get; private set; } = null!;
    private IGraph _metadataGraph = new Graph();
    private readonly TripleStore _store = new();
    private InMemoryDataset _dataset;
    private LeviathanQueryProcessor _queryProcessor;
    private string _nQuadsString;
    private readonly bool _ignoreDescribesConstraint;
    public List<Triple>? Metadata { get; private set; }
    public HashSet<string>? Scopes { get; private set; }
    public HashSet<string>? Describes { get; private set; }
    public List<string>? Replaces { get; private set; }
    public string? IsSubRecordOf { get; set; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public Record(ITripleStore store, bool ignoreDescribesConstraint = false)
    {
        _ignoreDescribesConstraint = ignoreDescribesConstraint;
        LoadFromTripleStore(store);
    }

    public Record(IGraph graph, bool ignoreDescribesConstraint = false)
    {
        _ignoreDescribesConstraint = ignoreDescribesConstraint;
        LoadFromGraph(graph);
    }

    public Record(string rdfString, bool ignoreDescribesConstraint = false)
    {
        _ignoreDescribesConstraint = ignoreDescribesConstraint;
        LoadFromString(rdfString);
    }

    public Record(string rdfString, IStoreReader reader, bool ignoreDescribesConstraint = false)
    {
        _ignoreDescribesConstraint = ignoreDescribesConstraint;
        LoadFromString(rdfString, reader);
    }

#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    private void LoadFromTripleStore(ITripleStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        if (store.Graphs.Count < 1) throw new RecordException("A record must contain at least one named graph.");

        foreach (var graph in store.Graphs) _store.Add(graph);

        _dataset = new InMemoryDataset(_store, false);
        _queryProcessor = new LeviathanQueryProcessor(_dataset);
        _queryProcessor = new LeviathanQueryProcessor(_dataset);

        _metadataGraph = FindMetadataGraph();

        Id = _metadataGraph.BaseUri?.ToString() ?? throw new RecordException("Metadata graph must have a base URI.");

        Metadata = [.. TriplesWithSubject(Id)];

        Scopes = [.. TriplesWithPredicate(Namespaces.Record.IsInScope).Select(q => q.Object.ToString()).OrderBy(s => s)];
        Describes = [.. TriplesWithPredicate(Namespaces.Record.Describes).Select(q => q.Object.ToString()).OrderBy(d => d)];

        Replaces = [.. TriplesWithPredicate(Namespaces.Record.Replaces).Select(q => q.Object.ToString())];

        var subRecordOf = TriplesWithPredicate(Namespaces.Record.IsSubRecordOf).Select(q => q.Object.ToString()).ToArray();
        if (subRecordOf.Length > 1)
            throw new RecordException("A record can at most be the subrecord of one other record.");

        IsSubRecordOf = subRecordOf.FirstOrDefault();

        if (!_ignoreDescribesConstraint)
            AssertDescribesConstraint();

        var rdfString = ToString(new NQuadsWriter(NQuadsSyntax.Rdf11));
        var sortedTriples = string.Join("\n", rdfString.Split('\n').OrderBy(s => s)); // <- Something is off about the canonlization of the RDF

        _nQuadsString = sortedTriples;
    }

    private void LoadFromString(string rdfString, IStoreReader reader)
    {
        var store = new TripleStore();
        if (!string.IsNullOrEmpty(Id) || !_metadataGraph.IsEmpty || Metadata != null)
            throw new RecordException("Record is already loaded.");

        store.LoadFromString(rdfString, reader);

        LoadFromTripleStore(store);
    }


    private void LoadFromString(string rdfString)
    {
        var store = new TripleStore();
        if (!string.IsNullOrEmpty(Id) || !_metadataGraph.IsEmpty || Metadata != null)
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
        if (graph.Name == null) throw new RecordException("The IGraph's name must be set.");
        var tempGraph = new Graph(graph.Name);
        tempGraph.Merge(graph);

        var tempStore = new TripleStore();
        tempStore.Add(tempGraph);

        LoadFromTripleStore(tempStore);
    }

    public IGraph MetadataGraph()
    {
        var tempGraph = new Graph(_metadataGraph.BaseUri);
        tempGraph.Merge(_metadataGraph);
        return tempGraph;
    }

    private void AssertDescribesConstraint()
    {
        if (AskIfNotAllDescribesNodesExistInContent())
            throw new RecordException("All described nodes on the metadata graph must exist as nodes on the content graph.");

        if (AskIfContentSubjectIsUnreachableFromMetadata())
            throw new RecordException("All nodes on the content graph must be reachable through the describes predicate on the metadata graph.");
    }

    private bool AskIfNotAllDescribesNodesExistInContent()
    {
        var parameterizedQuery = new SparqlParameterizedString(@"
            ASK {
                GRAPH ?metaGraph {
                    ?meta a @Record; 
                         @describes ?desc;
                         @hasContent ?content. }
                FILTER NOT EXISTS {
                { GRAPH ?content {?desc ?P ?O. } }
                    UNION 
                { GRAPH ?content {?S ?P ?desc. } } }
            }");

        parameterizedQuery.SetUri("Record", new Uri(Namespaces.Record.RecordType));
        parameterizedQuery.SetUri("describes", new Uri(Namespaces.Record.Describes));
        parameterizedQuery.SetUri("hasContent", new Uri(Namespaces.Record.HasContent));


        var parser = new SparqlQueryParser();
        var queryString = parameterizedQuery.ToString();
        var query = parser.ParseFromString(queryString);

        var queryResult = (SparqlResultSet)_queryProcessor.ProcessQuery(query);
        return queryResult.Result;
    }

    private bool AskIfContentSubjectIsUnreachableFromMetadata()
    {
        var parameterizedQuery = new SparqlParameterizedString(@"
            ASK {
                GRAPH ?metaGraph {
                    ?recId a @Record ;
                    @hasContent ?content. 
                }
                { GRAPH ?content {?unreachable ?P ?O. } }
                    UNION
                { GRAPH ?content {?S ?P ?unreachable . } }
                
                FILTER NOT EXISTS {
                    GRAPH ?metaGraph { ?recId @describes ?describedObject. }
                    GRAPH ?content { ?describedObject (^!@notConnected | !@notConnected)* ?unreachable . }
                    
                }
            }");

        parameterizedQuery.SetUri("Record", new Uri(Namespaces.Record.RecordType));
        parameterizedQuery.SetUri("describes", new Uri(Namespaces.Record.Describes));
        parameterizedQuery.SetUri("hasContent", new Uri(Namespaces.Record.HasContent));
        parameterizedQuery.SetUri("notConnected", new Uri(Namespaces.Record.NotConnected));

        var parser = new SparqlQueryParser();
        var queryString = parameterizedQuery.ToString();
        var query = parser.ParseFromString(queryString);

        var queryResult = (SparqlResultSet)_queryProcessor.ProcessQuery(query);
        return queryResult.Result;
    }

    private IGraph FindMetadataGraph()
    {
        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT { ?s ?p ?o . } WHERE { GRAPH ?g { ?s ?p ?o . ?g a @RecordType . } }");
        parameterizedQuery.SetUri("RecordType", new Uri(Namespaces.Record.RecordType));

        var parser = new SparqlQueryParser();
        var metadataQueryString = parameterizedQuery.ToString();
        var metadataQuery = parser.ParseFromString(metadataQueryString);

        var result = (IGraph)_queryProcessor.ProcessQuery(metadataQuery);
        if (result == null || result.IsEmpty) throw new RecordException("A record must have exactly one metadata graph.");

        // Extract the graph name from the result set
        var graphName = result.Triples.FirstOrDefault(t => t.Object.ToString().Equals(Namespaces.Record.RecordType))?.Subject.ToString();

        if (string.IsNullOrEmpty(graphName)) throw new RecordException("A record must have exactly one metadata graph.");

        result.BaseUri = new Uri(graphName);

        return result;
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

    public ITripleStore TripleStore()
    {
        var tempStore = new TripleStore();
        foreach (var graph in _store.Graphs) tempStore.Add(graph);

        return tempStore;
    }

    public IGraph GetMergedGraphs() => _store.Collapse(Id);

    public IEnumerable<IGraph> GetContentGraphs()
        => _store.Graphs.Where(g => g.Name?.ToString() != Id && !g.IsEmpty);

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


    public IEnumerable<INode> SubjectWithType(string type) => SubjectWithType(new Uri(type));
    public IEnumerable<INode> SubjectWithType(Uri type) => SubjectWithType(new UriNode(type));
    public IEnumerable<INode> SubjectWithType(INode type)
        => _store
        .GetTriplesWithPredicateObject(Namespaces.Rdf.UriNodes.Type, type)
        .Select(t => t.Subject);

    public IEnumerable<string> LabelsOfSubject(string subject) => LabelsOfSubject(new Uri(subject));
    public IEnumerable<string> LabelsOfSubject(Uri subject) => LabelsOfSubject(new UriNode(subject));
    public IEnumerable<string> LabelsOfSubject(INode subject)
        => _store
        .GetTriplesWithSubjectPredicate(subject, Namespaces.Rdfs.UriNodes.Label)
        .Where(t => t.Object is LiteralNode literal)
        .Select(t => ((LiteralNode)t.Object).Value);

    public IEnumerable<Triple> TriplesWithSubject(string subject) => TriplesWithSubject(new Uri(subject));
    public IEnumerable<Triple> TriplesWithSubject(Uri subject) => TriplesWithSubject(new UriNode(subject));
    public IEnumerable<Triple> TriplesWithSubject(INode subject)
        => _store
            .GetTriplesWithSubject(subject);
    public IEnumerable<Triple> TriplesWithPredicate(string predicate) => TriplesWithPredicate(new Uri(predicate));
    public IEnumerable<Triple> TriplesWithPredicate(Uri predicate) => TriplesWithPredicate(new UriNode(predicate));
    public IEnumerable<Triple> TriplesWithPredicate(INode predicate)
        => _store
            .GetTriplesWithPredicate(predicate);
    public IEnumerable<Triple> TriplesWithObject(string @object) => TriplesWithObject(new Uri(@object));
    public IEnumerable<Triple> TriplesWithObject(Uri @object) => TriplesWithObject(new UriNode(@object));
    public IEnumerable<Triple> TriplesWithObject(INode @object)
        => _store
            .GetTriplesWithObject(@object);

    public IEnumerable<Triple> TriplesWithPredicateAndObject(string predicate, string @object) => TriplesWithPredicateAndObject(new Uri(predicate), new Uri(@object));
    public IEnumerable<Triple> TriplesWithPredicateAndObject(Uri predicate, Uri @object) => TriplesWithPredicateAndObject(new UriNode(predicate), new UriNode(@object));
    public IEnumerable<Triple> TriplesWithPredicateAndObject(INode predicate, INode @object)
        => _store
            .GetTriplesWithPredicateObject(predicate, @object);


    public IEnumerable<Triple> TriplesWithSubjectObject(string subject, string @object) => TriplesWithSubjectObject(new Uri(subject), new Uri(@object));
    public IEnumerable<Triple> TriplesWithSubjectObject(Uri subject, Uri @object) => TriplesWithSubjectObject(new UriNode(subject), new UriNode(@object));

    public IEnumerable<Triple> TriplesWithSubjectObject(UriNode subject, UriNode @object)
        => _store
            .GetTriplesWithSubjectObject(subject, @object);

    public IEnumerable<Triple> TriplesWithSubjectPredicate(string subject, string predicate) => TriplesWithSubjectPredicate(new Uri(subject), new Uri(predicate));
    public IEnumerable<Triple> TriplesWithSubjectPredicate(Uri subject, Uri predicate) => TriplesWithSubjectPredicate(new UriNode(subject), new UriNode(predicate));

    public IEnumerable<Triple> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate)
        => _store
            .GetTriplesWithSubjectPredicate(subject, predicate);



    public IEnumerable<Triple> Triples() => _store.Triples ?? throw new UnloadedRecordException();
    public IEnumerable<Triple> ContentAsTriples()
    {
        var parser = new SparqlQueryParser();
        var metadata = MetadataAsTriples();

        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT {?s ?p ?o} WHERE { GRAPH @Id { ?s ?p ?o FILTER(?s != @Id)} }");
        parameterizedQuery.SetUri("Id", new Uri(Id));
        var contentQueryString = parameterizedQuery.ToString();
        var contentQuery = parser.ParseFromString(contentQueryString);
        var content = ConstructQuery(contentQuery);
        return content.Triples.Except(metadata);
    }

    public IEnumerable<Triple> MetadataAsTriples() => _metadataGraph.Triples;

    public bool ContainsTriple(Triple triple) => _store.Contains(triple);


    public override string ToString() => _nQuadsString;
    public string ToString<T>() where T : IStoreWriter, new() => ToString(new T());
    public string ToString(IStoreWriter writer)
    {
        var stringWriter = new StringWriter();
        writer.Save(_store, stringWriter);

        return stringWriter.ToString();
    }


    public string ToCanonString()
    {
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11);
        var canon = new RdfCanonicalizer().Canonicalize(_store);
        var canonStore = canon.OutputDataset;

        var stringWriter = new StringWriter();
        writer.Save(canonStore, stringWriter);

        var result = stringWriter.ToString();

        return result;
    }

    public bool Equals(Record? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Scopes!.SetEquals(other.Scopes!) && Describes!.SetEquals(other.Describes!);
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
        return resultGraph.Triples.Select(t => t.ToString());
    }

    private IEnumerable<string> Select(SparqlQuery query)
    {
        var rset = Query(query);
        foreach (var result in rset)
        {
            if (result is not null) yield return result.ToString()!;
        }
    }


    #endregion

}