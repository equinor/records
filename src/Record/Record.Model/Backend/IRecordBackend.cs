using Records.Immutable;
using VDS.RDF;
using VDS.RDF.Query;

namespace Records.Backend;

public interface IRecordBackend
{
    public Task<ITripleStore> TripleStore();
    public Uri GetRecordId();
    public Task<IGraph> GetMetadataGraph();
    public Task<string> ToString(RdfMediaType mediaType);
    public Task<IEnumerable<INode>> SubjectWithType(IUriNode type);

    public Task<IEnumerable<string>> LabelsOfSubject(IUriNode subject);
    public Task<IEnumerable<Triple>> TriplesWithSubject(IUriNode subject);
    public Task<IEnumerable<Triple>> TriplesWithPredicate(IUriNode predicate);
    public Task<IEnumerable<Triple>> TriplesWithPredicates(IEnumerable<IUriNode> predicates);
    public Task<IEnumerable<Triple>> TriplesWithObject(INode @object);
    public Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(IUriNode predicate, INode @object);
    public Task<IEnumerable<Triple>> TriplesWithSubjectObject(IUriNode subject, INode @object);
    public Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(IUriNode subject, IUriNode predicate);

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
    public Task<IGraph> ConstructQuery(SparqlQuery query);

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
    public Task<SparqlResultSet> Query(SparqlQuery query);

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
    public Task<IEnumerable<string>> Sparql(string queryString);

    Task<IGraph> GetMergedGraphs();
    Task<IEnumerable<IGraph>> GetContentGraphs();
    Task<IEnumerable<Triple>> Triples();
    Task<bool> ContainsTriple(Triple triple);
    Task<string> ToCanonString();
    ValueTask DeleteDatasetAsync();
    Task<IRecordBackend> CreateFromTripleStore(ITripleStore tripleStore);
    Task<ShaclValidationOutcome> ValidateContentWithShacl(IEnumerable<string> shaclShapePaths, string describesIri);
    Task<ShaclValidationOutcome> ValidateShacl(string content, RdfMediaType contentType, IEnumerable<string> shaclShapePaths);
    internal Task<IRecordBackend> WithAdditionalMetadata(IGraph additionalMetadata);
}

/// <summary>
/// Extends <see cref="IRecordBackend"/> with mutable, build-time operations used by
/// <see cref="Records.RecordBuilder"/> to populate the backend incrementally without
/// round-tripping through an intermediate TripleStore. Once all data is pushed, call
/// <see cref="FinalizeAsync"/> to build query indices and initialise metadata.
/// </summary>
public interface IRecordBuildableBackend : IRecordBackend
{
    /// <summary>Adds a fully-formed named graph to the backend store.</summary>
    Task AddGraphAsync(IGraph graph);

    /// <summary>
    /// Creates or extends a named graph with the supplied in-memory triples.
    /// </summary>
    Task AddTriplesToGraphAsync(Uri graphName, IEnumerable<Triple> triples);

    /// <summary>
    /// Parses an RDF string (Turtle / JSON-LD auto-detected) and loads all triples
    /// into the specified named graph.
    /// </summary>
    Task ParseRdfStringIntoGraphAsync(string rdfString, Uri graphName);

    /// <summary>
    /// Called once when all data has been pushed. Builds internal query indices and
    /// initialises the metadata index so the instance can be used as a read backend.
    /// </summary>
    Task FinalizeAsync();
}
