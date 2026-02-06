using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace Records.Backend;

public abstract class RecordBackendBase : IRecordBackend
{
    protected Uri? RecordId { get; private set; }
    protected IGraph? MetadataGraph { get; private set; }

    protected async Task InitializeMetadata()
    {
        MetadataGraph = await FindMetadataGraph();
        RecordId = MetadataGraph.BaseUri ?? throw new RecordException("Metadata graph must have a base URI.");
    }

    public Uri GetRecordId() => RecordId ?? throw new RecordException("RecordBackendBase not initialized. Call InitializeMetadata() in your constructor.");
    public Task<IGraph> GetMetadataGraph() => Task.FromResult(MetadataGraph ?? throw new RecordException("RecordBackendBase not initialized. Call InitializeMetadata() in your constructor."));

    private async Task<IGraph> FindMetadataGraph()
    {
        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT { ?s ?p ?o . } WHERE { GRAPH ?g { ?s ?p ?o . ?g a @RecordType . } }");
        parameterizedQuery.SetUri("RecordType", new Uri(Namespaces.Record.RecordType));

        var parser = new SparqlQueryParser();
        var metadataQueryString = parameterizedQuery.ToString();
        var metadataQuery = parser.ParseFromString(metadataQueryString);

        var result = await ((IRecordBackend)this).ConstructQuery(metadataQuery);
        if (result == null || result.IsEmpty) throw new RecordException("A record must have exactly one metadata graph.");

        var graphName = result.Triples.FirstOrDefault(t => t.Object.ToString().Equals(Namespaces.Record.RecordType))?.Subject.ToString();
        if (string.IsNullOrEmpty(graphName)) throw new RecordException("A record must have exactly one metadata graph.");

        result.BaseUri = new Uri(graphName);
        return result;
    }

    public abstract Task<ITripleStore> TripleStore();
    public abstract Task<string> ToString(RdfMediaType mediaType);
    public abstract Task<IEnumerable<INode>> SubjectWithType(UriNode type);
    public abstract Task<IEnumerable<string>> LabelsOfSubject(UriNode subject);
    public abstract Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject);
    public abstract Task<IEnumerable<Triple>> TriplesWithPredicate(UriNode predicate);
    public abstract Task<IEnumerable<Triple>> TriplesWithObject(INode @object);
    public abstract Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(UriNode predicate, INode @object);
    public abstract Task<IEnumerable<Triple>> TriplesWithSubjectObject(UriNode subject, INode @object);
    public abstract Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate);
    public abstract Task<IGraph> ConstructQuery(SparqlQuery query);
    public abstract Task<SparqlResultSet> Query(SparqlQuery query);
    public abstract Task<IEnumerable<string>> Sparql(string queryString);
    public abstract Task<IGraph> GetMergedGraphs();
    public abstract Task<IEnumerable<IGraph>> GetContentGraphs();
    public abstract Task<IEnumerable<Triple>> Triples();
    public abstract Task<bool> ContainsTriple(Triple triple);
    public abstract Task<string> ToCanonString();
}