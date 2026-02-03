using Newtonsoft.Json;
using Records.Exceptions;
using Records.Sender;
using System.Diagnostics;
using IriTools;
using Records.Backend;
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
    private IRecordBackend _backend;
    public readonly string Id;
    private readonly DescribesConstraintMode _describesConstraintMode;
    public List<Triple>? Metadata { get; private set; }
    public HashSet<string>? Scopes { get; private set; }
    public HashSet<string>? Related { get; private set; }
    public HashSet<string>? Describes { get; private set; }
    public List<string>? Replaces { get; private set; }
    public string? IsSubRecordOf { get; set; }


    public Record(IRecordBackend backend, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None)
    {
        _describesConstraintMode = describesConstraintMode;
        _backend = backend;
        Id = _backend.GetRecordId().ToString();
        Metadata = [.. TriplesWithSubject(Id)];

        Scopes = [.. TriplesWithPredicate(Namespaces.Record.IsInScope).Select(q => q.Object.ToString()).OrderBy(s => s)];
        Related = [.. TriplesWithPredicate(Namespaces.Record.Related).Select(q => q.Object.ToString()).OrderBy(r => r)];
        Describes = [.. TriplesWithPredicate(Namespaces.Record.Describes).Select(q => q.Object.ToString()).OrderBy(d => d)];
        Replaces = [.. TriplesWithPredicate(Namespaces.Record.Replaces).Select(q => q.Object.ToString())];

        var subRecordOf = TriplesWithPredicate(Namespaces.Record.IsSubRecordOf).Select(q => q.Object.ToString()).ToArray();
        if (subRecordOf.Length > 1)
            throw new RecordException("A record can at most be the subrecord of one other record.");

        IsSubRecordOf = subRecordOf.FirstOrDefault();

        ValidateDescribes();
    }
    public Record(ITripleStore store, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None)
           : this(new DotNetRdfRecordBackend(store), describesConstraintMode)
    {
    }
    public Record(string rdfString, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None)
           : this(new DotNetRdfRecordBackend(rdfString), describesConstraintMode)
    {
    }
    public Record(IGraph graph, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None)
           : this(new DotNetRdfRecordBackend(graph), describesConstraintMode)
    {
    }
    public Record(string rdfString, IStoreReader reader, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None)
           : this(new DotNetRdfRecordBackend(rdfString, reader), describesConstraintMode)
    {
    }


    public static implicit operator HttpRequestMessage(Record record)
    {
        var message = new HttpRequestMessage();
        message.AddRecord(record);
        return message;
    }


    private void ValidateDescribes()
    {
        switch (_describesConstraintMode)
        {
            case DescribesConstraintMode.None:
                break;
            case DescribesConstraintMode.DescribesIsInContent:
                AskIfNotAllDescribesNodesExistInContent();
                break;
            case DescribesConstraintMode.AllContentReachableFromDescribes:
                AskIfNotAllDescribesNodesExistInContent();
                AskIfContentSubjectIsUnreachableFromMetadata();
                break;
        }
    }

    public IEnumerable<string> Sparql(string queryString) => _backend.Sparql(queryString);

    public IGraph MetadataGraph()
    {
        var tempGraph = new Graph(_backend.GetMetadataGraph().BaseUri);
        tempGraph.Merge(_backend.GetMetadataGraph());
        return tempGraph;
    }

    private void AskIfNotAllDescribesNodesExistInContent()
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

        var queryResult = _backend.Query(query);
        if (queryResult.Result)
        {
            throw new RecordException("All described nodes on the metadata graph must exist as nodes on the content graph.");
        }
    }

    private void AskIfContentSubjectIsUnreachableFromMetadata()
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

        var queryResult = _backend.Query(query);

        if (queryResult.Result)
            throw new RecordException("All nodes on the content graph must be reachable through the describes predicate on the metadata graph.");

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

    public ITripleStore TripleStore() => _backend.TripleStore();

    public IGraph GetMergedGraphs() => _backend.GetMergedGraphs();

    public IEnumerable<IGraph> GetContentGraphs() => _backend.GetContentGraphs();





    public IEnumerable<INode> SubjectWithType(string type) => SubjectWithType(new Uri(type));
    public IEnumerable<INode> SubjectWithType(Uri type) => SubjectWithType(new UriNode(type));
    public IEnumerable<INode> SubjectWithType(UriNode type) => _backend.SubjectWithType(type);

    public IEnumerable<string> LabelsOfSubject(string subject) => LabelsOfSubject(new Uri(subject));
    public IEnumerable<string> LabelsOfSubject(Uri subject) => LabelsOfSubject(new UriNode(subject));
    public IEnumerable<string> LabelsOfSubject(UriNode subject) => _backend.LabelsOfSubject((subject));

    public IEnumerable<Triple> TriplesWithSubject(string subject) => TriplesWithSubject(new Uri(subject));
    public IEnumerable<Triple> TriplesWithSubject(Uri subject) => TriplesWithSubject(new UriNode(subject));
    public IEnumerable<Triple> TriplesWithSubject(UriNode subject) => _backend.TriplesWithSubject((subject));

    public IEnumerable<Triple> TriplesWithPredicate(string predicate) => TriplesWithPredicate(new Uri(predicate));
    public IEnumerable<Triple> TriplesWithPredicate(Uri predicate) => TriplesWithPredicate(new UriNode(predicate));
    public IEnumerable<Triple> TriplesWithPredicate(UriNode predicate) => _backend.TriplesWithPredicate((predicate));

    public IEnumerable<Triple> TriplesWithObject(string @object) => TriplesWithObject(new Uri(@object));
    public IEnumerable<Triple> TriplesWithObject(Uri @object) => TriplesWithObject(new UriNode(@object));
    public IEnumerable<Triple> TriplesWithObject(INode @object) => _backend.TriplesWithObject((@object));

    public IEnumerable<Triple> TriplesWithPredicateAndObject(string predicate, string @object) => TriplesWithPredicateAndObject(new Uri(predicate), new Uri(@object));
    public IEnumerable<Triple> TriplesWithPredicateAndObject(Uri predicate, Uri @object) => TriplesWithPredicateAndObject(new UriNode(predicate), new UriNode(@object));
    public IEnumerable<Triple> TriplesWithPredicateAndObject(UriNode predicate, INode @object) => _backend.TriplesWithPredicateAndObject((predicate), (@object));

    public IEnumerable<Triple> TriplesWithSubjectObject(string subject, string @object) => TriplesWithSubjectObject(new Uri(subject), new Uri(@object));
    public IEnumerable<Triple> TriplesWithSubjectObject(Uri subject, Uri @object) => TriplesWithSubjectObject(new UriNode(subject), new UriNode(@object));
    public IEnumerable<Triple> TriplesWithSubjectObject(UriNode subject, INode @object) => _backend.TriplesWithSubjectObject((subject), (@object));

    public IEnumerable<Triple> TriplesWithSubjectPredicate(string subject, string predicate) => TriplesWithSubjectPredicate(new Uri(subject), new Uri(predicate));
    public IEnumerable<Triple> TriplesWithSubjectPredicate(Uri subject, Uri predicate) => TriplesWithSubjectPredicate(new UriNode(subject), new UriNode(predicate));
    public IEnumerable<Triple> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate) => _backend.TriplesWithSubjectPredicate((subject), (predicate));



    public IEnumerable<Triple> Triples() => _backend.Triples();
    public IEnumerable<Triple> ContentAsTriples()
    {
        var parser = new SparqlQueryParser();
        var metadata = MetadataAsTriples();

        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT {?s ?p ?o} WHERE { GRAPH @Id { ?s ?p ?o FILTER(?s != @Id)} }");
        parameterizedQuery.SetUri("Id", new Uri(Id));
        var contentQueryString = parameterizedQuery.ToString();
        var contentQuery = parser.ParseFromString(contentQueryString);
        var content = _backend.ConstructQuery(contentQuery);
        return content.Triples.Except(metadata);
    }

    public IEnumerable<Triple> MetadataAsTriples() => _backend.GetMetadataGraph().Triples;

    public bool ContainsTriple(Triple triple) => _backend.ContainsTriple(triple);


    public override string? ToString() => _backend.ToString();
    public string ToString<T>() where T : IStoreWriter, new() => ToString(new T());
    public string ToString(IStoreWriter writer) => _backend.ToString(writer);


    public string ToCanonString() => _backend.ToCanonString();

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

}