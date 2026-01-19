using Records;
using Records.Backend;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

public abstract class RecordBackendBase : IRecordBackend
{
    protected Uri? RecordId { get; private set; }
    protected IGraph? MetadataGraph { get; private set; }

    protected void InitializeMetadata()
    {
        MetadataGraph = FindMetadataGraph();
        RecordId = MetadataGraph.BaseUri ?? throw new RecordException("Metadata graph must have a base URI.");
    }

    public Uri GetRecordId() => RecordId ?? throw new RecordException("RecordBackendBase not initialized. Call InitializeMetadata() in your constructor.");
    public IGraph GetMetadataGraph() => MetadataGraph ?? throw new RecordException("RecordBackendBase not initialized. Call InitializeMetadata() in your constructor.");

    private IGraph FindMetadataGraph()
    {
        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT { ?s ?p ?o . } WHERE { GRAPH ?g { ?s ?p ?o . ?g a @RecordType . } }");
        parameterizedQuery.SetUri("RecordType", new Uri(Namespaces.Record.RecordType));

        var parser = new SparqlQueryParser();
        var metadataQueryString = parameterizedQuery.ToString();
        var metadataQuery = parser.ParseFromString(metadataQueryString);

        var result = ((IRecordBackend)this).ConstructQuery(metadataQuery);
        if (result == null || result.IsEmpty) throw new RecordException("A record must have exactly one metadata graph.");

        var graphName = result.Triples.FirstOrDefault(t => t.Object.ToString().Equals(Namespaces.Record.RecordType))?.Subject.ToString();
        if (string.IsNullOrEmpty(graphName)) throw new RecordException("A record must have exactly one metadata graph.");

        result.BaseUri = new Uri(graphName);
        return result;
    }

    public abstract ITripleStore TripleStore();
    public abstract string ToString(IStoreWriter writer);
    public abstract IEnumerable<INode> SubjectWithType(UriNode type);
    public abstract IEnumerable<string> LabelsOfSubject(UriNode subject);
    public abstract IEnumerable<Triple> TriplesWithSubject(UriNode subject);
    public abstract IEnumerable<Triple> TriplesWithPredicate(UriNode predicate);
    public abstract IEnumerable<Triple> TriplesWithObject(INode @object);
    public abstract IEnumerable<Triple> TriplesWithPredicateAndObject(UriNode predicate, INode @object);
    public abstract IEnumerable<Triple> TriplesWithSubjectObject(UriNode subject, INode @object);
    public abstract IEnumerable<Triple> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate);
    public abstract IGraph ConstructQuery(SparqlQuery query);
    public abstract SparqlResultSet Query(SparqlQuery query);
    public abstract IEnumerable<string> Sparql(string queryString);
    public abstract IGraph GetMergedGraphs();
    public abstract IEnumerable<IGraph> GetContentGraphs();
    public abstract IEnumerable<Triple> Triples();
    public abstract bool ContainsTriple(Triple triple);
    public abstract string ToCanonString();
}
