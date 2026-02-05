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
    public HashSet<string>? Describes { get; private set; }
    public List<string>? Replaces { get; private set; }
    public string? IsSubRecordOf { get; set; }


    public Record(IRecordBackend backend, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None)
    {
        _describesConstraintMode = describesConstraintMode;
        _backend = backend;
        Id = _backend.GetRecordId().AbsoluteUri;
        Metadata = [.. TriplesWithSubject(Id).Result];

        Scopes = [.. TriplesWithPredicate(Namespaces.Record.IsInScope).Result.Select(q => q.Object.ToString()).OrderBy(s => s)];
        Describes = [.. TriplesWithPredicate(Namespaces.Record.Describes).Result.Select(q => q.Object.ToString()).OrderBy(d => d)];

        Replaces = [.. TriplesWithPredicate(Namespaces.Record.Replaces).Result.Select(q => q.Object.ToString())];

        var subRecordOf = TriplesWithPredicate(Namespaces.Record.IsSubRecordOf).Result.Select(q => q.Object.ToString()).ToArray();
        if (subRecordOf.Length > 1)
            throw new RecordException("A record can at most be the subrecord of one other record.");

        IsSubRecordOf = subRecordOf.FirstOrDefault();

        ValidateDescribes().Wait();
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
        message.AddRecord(record).RunSynchronously();
        return message;
    }


    private async Task ValidateDescribes()
    {
        switch (_describesConstraintMode)
        {
            case DescribesConstraintMode.None:
                break;
            case DescribesConstraintMode.DescribesIsInContent:
                await AskIfNotAllDescribesNodesExistInContent();
                break;
            case DescribesConstraintMode.AllContentReachableFromDescribes:
                await AskIfNotAllDescribesNodesExistInContent();
                await AskIfContentSubjectIsUnreachableFromMetadata();
                break;
        }
    }

    public Task<IEnumerable<string>> Sparql(string queryString) => _backend.Sparql(queryString);

    public async Task<IGraph> MetadataGraph()
    {
        var metadataGraph = await _backend.GetMetadataGraph();
        var tempGraph = new Graph(metadataGraph.BaseUri);
        tempGraph.Merge(metadataGraph);
        return tempGraph;
    }

    private async Task AskIfNotAllDescribesNodesExistInContent()
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

        var queryResult = await _backend.Query(query);
        if (queryResult.Result)
        {
            throw new RecordException("All described nodes on the metadata graph must exist as nodes on the content graph.");
        }
    }

    private async Task AskIfContentSubjectIsUnreachableFromMetadata()
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

        var queryResult = await _backend.Query(query);

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

    public Task<ITripleStore> TripleStore() => _backend.TripleStore();

    public Task<IGraph> GetMergedGraphs() => _backend.GetMergedGraphs();

    public Task<IEnumerable<IGraph>> GetContentGraphs() => _backend.GetContentGraphs();





    public Task<IEnumerable<INode>> SubjectWithType(string type) => SubjectWithType(new Uri(type));
    public Task<IEnumerable<INode>> SubjectWithType(Uri type) => SubjectWithType(new UriNode(type));
    public Task<IEnumerable<INode>> SubjectWithType(UriNode type) => _backend.SubjectWithType(type);

    public Task<IEnumerable<string>> LabelsOfSubject(string subject) => LabelsOfSubject(new Uri(subject));
    public Task<IEnumerable<string>> LabelsOfSubject(Uri subject) => LabelsOfSubject(new UriNode(subject));
    public Task<IEnumerable<string>> LabelsOfSubject(UriNode subject) => _backend.LabelsOfSubject((subject));

    public Task<IEnumerable<Triple>> TriplesWithSubject(string subject) => TriplesWithSubject(new Uri(subject));
    public Task<IEnumerable<Triple>> TriplesWithSubject(Uri subject) => TriplesWithSubject(new UriNode(subject));
    public Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject) => _backend.TriplesWithSubject((subject));

    public Task<IEnumerable<Triple>> TriplesWithPredicate(string predicate) => TriplesWithPredicate(new Uri(predicate));
    public Task<IEnumerable<Triple>> TriplesWithPredicate(Uri predicate) => TriplesWithPredicate(new UriNode(predicate));
    public Task<IEnumerable<Triple>> TriplesWithPredicate(UriNode predicate) => _backend.TriplesWithPredicate((predicate));

    public Task<IEnumerable<Triple>> TriplesWithObject(string @object) => TriplesWithObject(new Uri(@object));
    public Task<IEnumerable<Triple>> TriplesWithObject(Uri @object) => TriplesWithObject(new UriNode(@object));
    public Task<IEnumerable<Triple>> TriplesWithObject(INode @object) => _backend.TriplesWithObject((@object));

    public Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(string predicate, string @object) => TriplesWithPredicateAndObject(new Uri(predicate), new Uri(@object));
    public Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(Uri predicate, Uri @object) => TriplesWithPredicateAndObject(new UriNode(predicate), new UriNode(@object));
    public Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(UriNode predicate, INode @object) => _backend.TriplesWithPredicateAndObject((predicate), (@object));

    public Task<IEnumerable<Triple>> TriplesWithSubjectObject(string subject, string @object) => TriplesWithSubjectObject(new Uri(subject), new Uri(@object));
    public Task<IEnumerable<Triple>> TriplesWithSubjectObject(Uri subject, Uri @object) => TriplesWithSubjectObject(new UriNode(subject), new UriNode(@object));
    public Task<IEnumerable<Triple>> TriplesWithSubjectObject(UriNode subject, INode @object) => _backend.TriplesWithSubjectObject((subject), (@object));

    public Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(string subject, string predicate) => TriplesWithSubjectPredicate(new Uri(subject), new Uri(predicate));
    public Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(Uri subject, Uri predicate) => TriplesWithSubjectPredicate(new UriNode(subject), new UriNode(predicate));
    public Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate) => _backend.TriplesWithSubjectPredicate((subject), (predicate));



    public Task<IEnumerable<Triple>> Triples() => _backend.Triples();
    public async Task<IEnumerable<Triple>> ContentAsTriples()
    {
        var parser = new SparqlQueryParser();
        var metadata = await MetadataAsTriples();

        var parameterizedQuery = new SparqlParameterizedString("CONSTRUCT {?s ?p ?o} WHERE { GRAPH @Id { ?s ?p ?o FILTER(?s != @Id)} }");
        parameterizedQuery.SetUri("Id", new Uri(Id));
        var contentQueryString = parameterizedQuery.ToString();
        var contentQuery = parser.ParseFromString(contentQueryString);
        var content = await _backend.ConstructQuery(contentQuery);
        return content.Triples.Except(metadata);
    }

    public async Task<IEnumerable<Triple>> MetadataAsTriples() => (await _backend.GetMetadataGraph()).Triples;

    public Task<bool> ContainsTriple(Triple triple) => _backend.ContainsTriple(triple);


    public override string? ToString() => _backend.ToString();
    public Task<string> ToString<T>() where T : IStoreWriter, new() => ToString(new T());
    public Task<string> ToString(IStoreWriter writer) => _backend.ToString(writer);


    public Task<string> ToCanonString() => _backend.ToCanonString();

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