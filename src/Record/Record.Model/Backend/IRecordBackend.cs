using Lucene.Net.Util;
using VDS.RDF;
using VDS.RDF.Query;

namespace Records.Backend;

public interface IRecordBackend
{
    public ITripleStore TripleStore();
    public Uri GetRecordId();
    public IGraph GetMetadataGraph();
    public string ToString();
    public string ToString(IStoreWriter writer);
    public IEnumerable<INode> SubjectWithType(Uri type);

    public IEnumerable<string> LabelsOfSubject(Uri subject);

    public IEnumerable<Triple> TriplesWithSubject(Uri subject);
    public IEnumerable<Triple> TriplesWithPredicate(Uri predicate);
    public IEnumerable<Triple> TriplesWithObject(Uri @object);
    public IEnumerable<Triple> TriplesWithPredicateAndObject(Uri predicate, Uri @object);
    public IEnumerable<Triple> TriplesWithSubjectObject(Uri subject, Uri @object);
    public IEnumerable<Triple> TriplesWithSubjectPredicate(Uri subject, Uri predicate);

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
    public IGraph ConstructQuery(SparqlQuery query);
    
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
    public SparqlResultSet Query(SparqlQuery query);

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
    public IEnumerable<string> Sparql(string queryString);

    IGraph GetMergedGraphs();
    IEnumerable<IGraph> GetContentGraphs();
    IEnumerable<Triple> Triples();
    bool ContainsTriple(Triple triple);
    string ToCanonString();
}