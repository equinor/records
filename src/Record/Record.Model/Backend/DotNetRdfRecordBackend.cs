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

public class DotNetRdfRecordBackend : RecordBackendBase, IEquatable<DotNetRdfRecordBackend>
{

    private IGraph _metadataGraph;
    private readonly TripleStore _store;
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
        
        var rdfString = ToString(new NQuadsWriter(NQuadsSyntax.Rdf11));
        var sortedTriples = string.Join("\n", rdfString.Split('\n').OrderBy(s => s)); // <- Something is off about the canonlization of the RDF

        _nQuadsString = sortedTriples;
        
        InitializeMetadata();
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

    public IGraph MetadataGraph()
    {
        var tempGraph = new Graph(_metadataGraph.BaseUri);
        tempGraph.Merge(_metadataGraph);
        return tempGraph;
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

    public override ITripleStore TripleStore()
    {
        var tempStore = new TripleStore();
        foreach (var graph in _store.Graphs) tempStore.Add(graph);

        return tempStore;
    }

    public override IGraph GetMergedGraphs() => _store.Collapse(GetRecordId());

    public override IEnumerable<IGraph> GetContentGraphs()
        => _store.Graphs.Where(g => g.Name?.ToString() != GetRecordId().ToString() && !g.IsEmpty);

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
    public override IEnumerable<string> Sparql(string queryString)
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
    public override SparqlResultSet Query(SparqlQuery query) =>
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
    public override IGraph ConstructQuery(SparqlQuery query) =>
        _queryProcessor.ProcessQuery(query) switch
        {
            IGraph res => res,
            _ => throw new ArgumentException(
                "DotNetRdf did not return IGraph. Probably the Sparql query was not a construct query")
        };


    public override IEnumerable<INode> SubjectWithType(UriNode type)
        => _store
        .GetTriplesWithPredicateObject(Namespaces.Rdf.UriNodes.Type, type)
        .Select(t => t.Subject);

    public override IEnumerable<string> LabelsOfSubject(UriNode subject)
        => _store
        .GetTriplesWithSubjectPredicate(subject, Namespaces.Rdfs.UriNodes.Label)
        .Where(t => t.Object is LiteralNode literal)
        .Select(t => ((LiteralNode)t.Object).Value);

    public override IEnumerable<Triple> TriplesWithSubject(UriNode subject)
        => _store
            .GetTriplesWithSubject(subject);
    public override IEnumerable<Triple> TriplesWithPredicate(UriNode predicate)
        => _store
            .GetTriplesWithPredicate(predicate);
    
    public override IEnumerable<Triple> TriplesWithObject(INode @object)
        => _store
            .GetTriplesWithObject(@object);

    public override IEnumerable<Triple> TriplesWithPredicateAndObject(UriNode predicate, INode @object)
        => _store
            .GetTriplesWithPredicateObject(predicate, @object);


    public override IEnumerable<Triple> TriplesWithSubjectObject(UriNode subject, INode @object)
        => _store
            .GetTriplesWithSubjectObject(subject, @object);

    public override IEnumerable<Triple> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate)
        => _store
            .GetTriplesWithSubjectPredicate(subject, predicate);



    public override IEnumerable<Triple> Triples() => _store.Triples ?? throw new UnloadedRecordException();
    

    public IEnumerable<Triple> MetadataAsTriples() => _metadataGraph.Triples;

    public override bool ContainsTriple(Triple triple) => _store.Contains(triple);


    public override string ToString() => _nQuadsString;
    public string ToString<T>() where T : IStoreWriter, new() => ToString(new T());
    public override string ToString(IStoreWriter writer)
    {
        var stringWriter = new StringWriter();
        writer.Save(_store, stringWriter);

        return stringWriter.ToString();
    }


    public override string ToCanonString()
    {
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11);
        var canon = new RdfCanonicalizer().Canonicalize(_store);
        var canonStore = canon.OutputDataset;

        var stringWriter = new StringWriter();
        writer.Save(canonStore, stringWriter);

        var result = stringWriter.ToString();

        return result;
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