using VDS.RDF;

namespace Records.Tests;
public static class TestData
{
    public static Mutable.Record MutableRecordWithContent(string? id = null)
    {
        id ??= CreateRecordId("1");

        return new Mutable.Record(id)
            .WithAdditionalQuads(CreateQuadList(10, id).ToArray());
    }

    public static Immutable.Record ValidRecord(string? id = null)
    {
        id ??= CreateRecordId("1");
        
        var scopes = CreateObjectList(5, "scope");
        var describes = CreateObjectList(5, "describes");
        var content = CreateQuadList(10, id);

        return new RecordBuilder()
            .WithId(id)
            .WithScopes(scopes)
            .WithDescribes(describes)
            .WithContent(content)
            .Build();
    }

    public static string ValidJsonLdRecordString() => ValidRecordString<JsonLdRecordWriter>();
    public static string ValidNQuadRecordString() => ValidRecordString<NQuadsRecordWriter>();

    public static string ValidRecordString<T>() where T : IRdfWriter, new()
    {
        var record = ValidRecord();
        return record.ToString<T>();
    }

    public static List<string> CreateObjectList(int numberOfObjects, string subset)
        => Enumerable.Range(1, numberOfObjects)
            .Select(i => CreateRecordIri(subset, i.ToString()))
            .ToList();

    public static List<SafeQuad> CreateQuadList(int numberOfQuads, string graphLabel)
        => Enumerable.Range(1, 10)
            .Select(i =>
            {
                var (s, p, o) = CreateRecordTriple(i.ToString());
                return Quad.CreateSafe(s, p, o, graphLabel);
            })
            .ToList();

    public static string CreateRecordId(string id) => $"https://ssi.example.com/record/{id}";
    public static string CreateRecordSubject(string subject) => CreateRecordIri("subject", subject);
    public static string CreateRecordPredicate(string predicate) => CreateRecordIri("predicate", predicate);
    public static string CreateRecordObject(string @object) => CreateRecordIri("object", @object);
    public static string CreateRecordBlankNode(string blankNode) => $"_:{blankNode}";

    public static (string subject, string predicate, string @object) CreateRecordTriple(string id)
    {
        return (CreateRecordSubject(id), CreateRecordPredicate(id), CreateRecordObject(id));
    }

    public static (string subject, string predicate, string @object, string graphLabel) CreateRecordQuad(string id)
    {
        return (CreateRecordSubject(id), CreateRecordPredicate(id), CreateRecordObject(id), CreateRecordId(id));
    }

    public static string CreateRecordIri(string subset, string id) => $"https://ssi.example.com/{subset}/{id}";
}

