using Lucene.Net.Util;
using VDS.RDF;
using VDS.RDF.Query;

namespace Records.Backend;

public interface IRecordBackend
{
    public Task<ITripleStore> TripleStore();
    public Uri GetRecordId();
    public Task<IGraph> GetMetadataGraph();
    public Task<string> ToString(RdfMediaType mediaType);
    public Task<IEnumerable<INode>> SubjectWithType(UriNode type);

    public Task<IEnumerable<string>> LabelsOfSubject(UriNode subject);
    public Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject);
    public Task<IEnumerable<Triple>> TriplesWithPredicate(UriNode predicate);
    public Task<IEnumerable<Triple>> TriplesWithObject(INode @object);
    public Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(UriNode predicate, INode @object);
    public Task<IEnumerable<Triple>> TriplesWithSubjectObject(UriNode subject, INode @object);
    public Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate);

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
}