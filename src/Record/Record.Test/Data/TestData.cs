using VDS.RDF;
using VDS.RDF.Writing;

namespace Records.Tests;
public static class TestData
{

    public static IGraph CreateGraph(string id, int triples = 10)
    {
        var graph = new Graph(new Uri(id));

        var guid = Guid.NewGuid().ToString();

        Enumerable.Range(1, triples)
            .ToList().ForEach(i =>
            {
                var (s, p, o) = CreateRecordTripleStringTuple($"{guid}/{i}");
                graph.Assert(new Triple(new UriNode(new Uri(s)), new UriNode(new Uri(p)), new UriNode(new Uri(o))));
            });

        return graph;
    }

    public static Immutable.Record ValidRecord(string? id = null, int numberScopes = 5, int numberDescribes = 5, int numberQuads = 10)
    {
        return ValidRecordBeforeBuildComplete(id, numberScopes, numberDescribes, numberQuads).Build();
    }

    public static RecordBuilder ValidRecordBeforeBuildComplete(string? id = null, int numberScopes = 5,
        int numberDescribes = 5, int numberQuads = 10)
    {
        id ??= CreateRecordId("1");

        var content = CreateTripleList(numberQuads, id);
        return RecordBuilderWithProvenanceAndWithoutContent(id, numberScopes, numberDescribes, numberQuads)
            .WithContent(content);
    }

    public static RecordBuilder RecordBuilderWithProvenanceAndWithoutContent(string? id = null, int numberScopes = 5,
    int numberDescribes = 5, int numberQuads = 10, RecordCanonicalisation canon = RecordCanonicalisation.dotNetRdf)
    {
        id ??= CreateRecordId("1");

        var scopes = CreateObjectList(numberScopes, "scope");
        var describes = CreateObjectList(numberDescribes, "describes");

        return new RecordBuilder(canon, ignoreDescribesConstraint: true)
            .WithId(id)
            .WithScopes(scopes)
            .WithDescribes(describes)
            .WithAdditionalContentProvenance(ProvenanceBuilder.WithAdditionalTool("https://example.com/software/v1"));
    }

    public static string ValidJsonLdRecordString(string? id = null, int numberScopes = 5, int numberDescribes = 5, int numberQuads = 10)
        => ValidRecordString<JsonLdWriter>(id, numberScopes, numberDescribes, numberQuads);
    public static string ValidNQuadRecordString(string? id = null, int numberScopes = 5, int numberDescribes = 5, int numberQuads = 10)
        => ValidRecordString<NQuadsWriter>(id, numberScopes, numberDescribes, numberQuads);

    public static string ValidRecordString<T>(string? id = null, int numberScopes = 5, int numberDescribes = 5, int numberQuads = 10) where T : IStoreWriter, new()
    {
        var record = ValidRecord(id, numberScopes, numberDescribes, numberQuads);
        return record.ToString<T>();
    }

    public static List<string> CreateObjectList(int numberOfObjects, string subset)
        => Enumerable.Range(1, numberOfObjects)
            .Select(i => CreateRecordIri(subset, i.ToString()))
            .ToList();

    public static List<Triple> CreateTripleList(int numberOfQuads, string graphLabel)
        => Enumerable.Range(1, numberOfQuads)
            .Select(i =>
            {
                var (s, p, o) = CreateRecordTripleStringTuple(i.ToString());
                return new Triple(new UriNode(new Uri(s)), new UriNode(new Uri(p)), new UriNode(new Uri(o)));
            })
            .ToList();

    public static string CreateRecordId(string id) => $"https://ssi.example.com/record/{id}";
    public static Uri CreateRecordIdUri(string id) => new Uri(CreateRecordId(id));
    public static UriNode CreateRecordIdUriNode(string id) => new UriNode(CreateRecordIdUri(id));

    public static string CreateRecordId(int id) => CreateRecordId(id.ToString());
    public static string CreateRecordSubject(string subject) => CreateRecordIri("subject", subject);
    public static string CreateRecordPredicate(string predicate) => CreateRecordIri("predicate", predicate);
    public static string CreateRecordObject(string @object) => CreateRecordIri("object", @object);
    public static string CreateRecordBlankNode(string blankNode) => $"_:{blankNode}";

    public static Triple CreateRecordTriple(string id)
    {
        var (s, p, o) = CreateRecordTripleStringTuple(id);
        return new Triple(new UriNode(new Uri(s)), new UriNode(new Uri(p)), new UriNode(new Uri(o)));
    }

    public static (string subject, string predicate, string @object) CreateRecordTripleStringTuple(string id)
    {
        return (CreateRecordSubject(id), CreateRecordPredicate(id), CreateRecordObject(id));
    }

    public static (string subject, string predicate, string @object, string graphLabel) CreateRecordQuadStringTuple(string id)
    {
        return (CreateRecordSubject(id), CreateRecordPredicate(id), CreateRecordObject(id), CreateRecordId(id));
    }

    public static string CreateRecordIri(string subset, string id) => $"https://ssi.example.com/{subset}/{id}";

    public static string PutStringInAngleBrackets(string node) => $"<{node}>";
}

