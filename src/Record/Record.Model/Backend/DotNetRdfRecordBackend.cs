using Newtonsoft.Json;
using Records.Exceptions;
using Records.Sender;
using System.Diagnostics;
using Records.Backend;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;
namespace Records.Backend;

public class DotNetRdfRecordBackend : RecordBackendBase
{

    private readonly TripleStore _store = new TripleStore();
    private InMemoryDataset _dataset;
    private LeviathanQueryProcessor _queryProcessor;
    private string _nQuadsString;


#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public DotNetRdfRecordBackend(ITripleStore store)
    {
        LoadFromTripleStore(store);
    }

    public DotNetRdfRecordBackend(IGraph graph)
    {
        LoadFromGraph(graph);
    }

    public DotNetRdfRecordBackend(string rdfString)
    {
        LoadFromString(rdfString);
    }

    public DotNetRdfRecordBackend(string rdfString, IStoreReader reader)
    {
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

        var rdfString = ToString(new NQuadsWriter(NQuadsSyntax.Rdf11)).Result;
        var sortedTriples = string.Join("\n", rdfString.Split('\n').OrderBy(s => s)); // <- Something is off about the canonlization of the RDF

        _nQuadsString = sortedTriples;

        InitializeMetadata().Wait();
    }

    private void LoadFromString(string rdfString, IStoreReader reader)
    {
        var store = new TripleStore();

        store.LoadFromString(rdfString, reader);

        LoadFromTripleStore(store);
    }

    private void LoadFromString(string rdfString)
    {
        var store = new TripleStore();

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
        var tempGraph = new Graph(graph.Name);
        tempGraph.Merge(graph);

        var tempStore = new TripleStore();
        tempStore.Add(tempGraph);

        LoadFromTripleStore(tempStore);
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

    public override Task<ITripleStore> TripleStore()
    {
        var tempStore = new TripleStore();
        foreach (var graph in _store.Graphs) tempStore.Add(graph);

        return Task.FromResult((ITripleStore)tempStore);
    }

    public override Task<string> ToString(RdfMediaType mediaType)
    {
        return ToString(mediaType.GetStoreWriter());
    }

    public override Task<IGraph> GetMergedGraphs() => Task.FromResult(_store.Collapse(GetRecordId()));

    public override Task<IEnumerable<IGraph>> GetContentGraphs()
        => Task.FromResult(_store.Graphs.Where(g => g.Name?.ToString() != GetRecordId().AbsoluteUri && !g.IsEmpty));

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
    public override Task<IEnumerable<string>> Sparql(string queryString)
    {
        var command = queryString.Split().First();
        var parser = new SparqlQueryParser();
        var query = parser.ParseFromString(queryString);

        return command.ToLower() switch
        {
            "construct" => Task.FromResult(Construct(query)),
            "select" => Task.FromResult(Select(query)),
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
    public override Task<SparqlResultSet> Query(SparqlQuery query) =>
        _queryProcessor.ProcessQuery(query) switch
        {
            SparqlResultSet res => Task.FromResult(res),
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
    public override Task<IGraph> ConstructQuery(SparqlQuery query) =>
        _queryProcessor.ProcessQuery(query) switch
        {
            IGraph res => Task.FromResult(res),
            _ => throw new ArgumentException(
                "DotNetRdf did not return IGraph. Probably the Sparql query was not a construct query")
        };


    public override Task<IEnumerable<INode>> SubjectWithType(UriNode type)
        => Task.FromResult(_store
        .GetTriplesWithPredicateObject(Namespaces.Rdf.UriNodes.Type, type)
        .Select(t => t.Subject));

    public override Task<IEnumerable<string>> LabelsOfSubject(UriNode subject)
        => Task.FromResult(_store
        .GetTriplesWithSubjectPredicate(subject, Namespaces.Rdfs.UriNodes.Label)
        .Where(t => t.Object is LiteralNode literal)
        .Select(t => ((LiteralNode)t.Object).Value));

    public override Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject)
        => Task.FromResult(_store
            .GetTriplesWithSubject(subject));
    public override Task<IEnumerable<Triple>> TriplesWithPredicate(UriNode predicate)
        => Task.FromResult(_store
            .GetTriplesWithPredicate(predicate));

    public override Task<IEnumerable<Triple>> TriplesWithObject(INode @object)
        => Task.FromResult(_store
            .GetTriplesWithObject(@object));

    public override Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(UriNode predicate, INode @object)
        => Task.FromResult(_store
            .GetTriplesWithPredicateObject(predicate, @object));


    public override Task<IEnumerable<Triple>> TriplesWithSubjectObject(UriNode subject, INode @object)
        => Task.FromResult(_store
            .GetTriplesWithSubjectObject(subject, @object));

    public override Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate)
        => Task.FromResult(_store
            .GetTriplesWithSubjectPredicate(subject, predicate));



    public override Task<IEnumerable<Triple>> Triples() => Task.FromResult(_store.Triples ?? throw new UnloadedRecordException());


    public override Task<bool> ContainsTriple(Triple triple) => Task.FromResult(_store.Contains(triple));


    public override string ToString() => _nQuadsString;
    public Task<string> ToString<T>() where T : IStoreWriter, new() => ToString(new T());
    public Task<string> ToString(IStoreWriter writer)
    {
        var stringWriter = new StringWriter();
        writer.Save(_store, stringWriter);

        return Task.FromResult(stringWriter.ToString());
    }


    public override Task<string> ToCanonString()
    {
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11);
        var canon = new RdfCanonicalizer().Canonicalize(_store);
        var canonStore = canon.OutputDataset;

        var stringWriter = new StringWriter();
        writer.Save(canonStore, stringWriter);

        var result = stringWriter.ToString();

        return Task.FromResult(result);
    }

    public bool Equals(DotNetRdfRecordBackend? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return _dataset.Equals(other._dataset);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != GetType()) return false;
        return Equals((DotNetRdfRecordBackend)obj);
    }


    #region Private
    private IEnumerable<string> Construct(SparqlQuery query)
    {
        var resultGraph = ConstructQuery(query).Result;
        return resultGraph.Triples.Select(t => t.ToString());
    }

    private IEnumerable<string> Select(SparqlQuery query)
    {
        var rset = Query(query).Result;
        foreach (var result in rset)
        {
            if (result is not null) yield return result.ToString()!;
        }
    }

    public override int GetHashCode()
    {
        throw new NotImplementedException();
    }
    #endregion


}