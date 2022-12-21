using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Record = Records.Immutable.Record;
using Records.Exceptions;
using VDS.RDF.Writing;

namespace Records.Tests;

public class ImmutableRecordTests
{
    public const string rdf = @"
{
    ""@context"": {
        ""@version"": 1.1,
        ""@vocab"": ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""@base"":  ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""record"": ""https://rdf.equinor.com/ontology/record/"",
        ""eqn"": ""https://rdf.equinor.com/fam/"",
        ""akso"": ""https://akersolutions.com/data/"",
        ""record:isInScope"": { ""@type"": ""@id"" },
        ""record:describes"": { ""@type"": ""@id"" } 
    },
    ""@id"": ""akso:RecordID123"",
    ""@graph"": [
        {
            ""@id"": ""akso:RecordID123"",
            ""@type"": ""record:Record"",
            ""record:replaces"": ""https://ssi.example.com/record/0"",
            ""record:isInScope"": [
                ""eqn:TestScope1"",
                ""eqn:TestScope2""
            ],
            ""record:describes"": [
                ""eqn:Document/Wist/C277-AS-W-LA-00001.F01""
            ]
        },
        {
            ""@id"": ""eqn:Document/Wist/C277-AS-W-LA-00001.F01"",
            ""@type"": ""eqn:Revision"",
            ""RevisionSequence"": ""01"",
            ""Revision"": ""F01"",
            ""ReasonForIssue"": ""Revision text"",
            ""Author"": ""Kari Nordkvinne"",
            ""CheckedBy"": ""NN"",
            ""DisciplineApprovedBy"": ""NM"",
            ""DoubleHatTester"": ""Hei der ^^ eposten min er epost@example.com!"",
            ""NestedAtTester"": {
                ""@value"": ""Hei der ^^ eposten min er epost@example.com"",
                ""@language"": ""no-nn""
            }
        }
    ]
}
        ";

    public const string rdf2 = @"
{
    ""@context"": {
        ""@version"": 1.1,
        ""@vocab"": ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""@base"":  ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""record"": ""https://rdf.equinor.com/ontology/record/"",
        ""eqn"": ""https://rdf.equinor.com/fam/"",
        ""akso"": ""https://akersolutions.com/data/""
    },
    ""@id"": ""akso:RecordID123"",
    ""@graph"": [
        {
            ""@id"": ""akso:RecordID123"",
            ""record:replaces"": ""https://ssi.example.com/record/0"",
            ""record:isInScope"": [
                ""eqn:TestScope1"",
                ""eqn:TestScope2""
            ],
            ""record:describes"": [
                ""eqn:Document/Wist/C277-AS-W-LA-00001.F01""
            ]
        },
        {
            ""@id"": ""eqn:Document/WIST/C277-AS-W-LA-00001.F01"",
            ""@type"": ""eqn:Revision"",
            ""RevisionSequence"": ""01"",
            ""Revision"": ""F01"",
            ""ReasonForIssue"": ""Revision text"",
            ""Author"": ""Kari Nordkvinne"",
            ""CheckedBy"": ""NN"",
            ""DisciplineApprovedBy"": ""NM""
        }
    ]
}
        ";

    public const string rdf3 = @"<http://example.com/data/Object1/Record0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/record/Record> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/describes> <http://example.com/data/Object1> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isInScope> <http://example.com/data/Project> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/replaces> <http://ssi.example.com/record/0> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/mel/System> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Length> ""0"" <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Weight> ""0"" <http://example.com/data/Object1/Record0> .";

    public const string rdf4 = @"<http://example.com/data/Object1/Record0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/record/Record> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/describes> <http://example.com/data/Object1> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isInScope> <http://example.com/data/Project> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/replaces> <http://ssi.example.com/record/0> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isSubRecordOf> <http://ssi.example.com/record/original> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/mel/System> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Length> ""0"" <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Weight> ""0"" <http://example.com/data/Object1/Record0> .";

    public const string rdf5 = @"<http://example.com/data/Object1/Record0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/record/Record> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/describes> <http://example.com/data/Object1> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isInScope> <http://example.com/data/Project> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/replaces> <http://ssi.example.com/record/0> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isSubRecordOf> <http://ssi.example.com/record/original> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isSubRecordOf> <http://ssi.example.com/record/more-original> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/mel/System> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Length> ""0"" <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Weight> ""0"" <http://example.com/data/Object1/Record0> .";

    [Fact]
    public void Record_Has_Provenance()
    {
        var record = new Record(rdf);
        var result = record.Provenance.Count();

        result.Should().Be(5);
    }

    [Fact]
    public void Record_Finds_Id()
    {
        var record = new Record(rdf);
        var result = record.Id;

        result.Should().Be("https://akersolutions.com/data/RecordID123");
    }

    [Fact]
    public void Record_Can_Do_Queries()
    {
        var record = new Record(rdf);
        var queryResult = record.Sparql("construct { ?s ?p ?o } where { ?s ?p ?o . ?s <https://rdf.equinor.com/ontology/fam/v1/Author> ?o .}");
        var result = queryResult.Count();
        result.Should().Be(1);
    }

    [Fact]
    public void Get_All_Triples()
    {
        var record = new Record(rdf);
        var queryResult = record.Sparql("construct { ?s ?p ?o. } where { ?s ?p ?o . }");
        var result = queryResult.Count();

        result.Should().Be(14);
    }

    [Fact]
    public void Record_Does_Not_Have_Provenance()
    {
        var result = () => new Record(rdf2);
        result.Should().Throw<RecordException>().WithMessage("Failure in record. A record must have exactly one provenance object.");
    }

    [Fact]
    public void Record_Can_Be_Serialised_Nquad()
    {
        var record = new Record(rdf);

        var result = record.ToString<NQuadsRecordWriter>().Split("\n").Length;

        result.Should().Be(15);
    }

    [Fact]
    public void Record_Can_Be_Serialised_Turtle()
    {
        var record = new Record(rdf);

        var result = record.ToString<TurtleWriter>().Split("\n").Length;

        result.Should().Be(21);
    }

    [Fact]
    public void Record_Can_Produce_Quads()
    {
        var record = new Record(rdf);
        var result = record.Quads().Count();

        result.Should().Be(14);
    }

    [Fact]
    public void Record_Can_Produce_Quads_For_Specific_Subjects_Predicates_Or_Objects()
    {
        var record = new Record(rdf);

        var subjectCountResult = record.QuadsWithSubject("https://rdf.equinor.com/fam/Document/Wist/C277-AS-W-LA-00001.F01").Count();
        var predicateCountResult = record.QuadsWithPredicate("https://rdf.equinor.com/ontology/record/isInScope").Count();
        var objectCountResult = record.QuadsWithObject("https://rdf.equinor.com/fam/Document/Wist/C277-AS-W-LA-00001.F01").Count();

        subjectCountResult.Should().Be(9);
        predicateCountResult.Should().Be(2);
        objectCountResult.Should().Be(1);
    }

    [Fact]
    public void Record_Has_Scopes_And_Describes()
    {
        var record = new Record(rdf);

        var scopes = record.Scopes;
        var describes = record.Describes;

        var scopeCount = scopes.Count();
        var describesCount = describes.Count();

        scopeCount.Should().Be(2);
        describesCount.Should().Be(1);
    }

    [Fact]
    public void Record_With_Same_Scopes_And_Describes_Are_Equal()
    {
        var rdfString1 = RandomRecord(id: "1", numberScopes: 3, numberDescribes: 2);
        var rdfString2 = RandomRecord(id: "2", numberScopes: 3, numberDescribes: 2);

        var record = new Record(rdfString1);
        var record2 = new Record(rdfString2);

        record.Should().Be(record2);
    }

    [Fact]
    public void Record_With_Different_Scopes_And_Describes_Are_Not_Equal()
    {
        var rdfString1 = RandomRecord(id: "1", numberScopes: 3, numberDescribes: 2);
        var rdfString2 = RandomRecord(id: "2", numberScopes: 3, numberDescribes: 2);

        var record = new Record(rdfString1);
        var record2 = new Record(rdfString2);

        record.Should().Be(record2);

        var rdfString3 = RandomRecord(id: "3", numberScopes: 2, numberDescribes: 2);
        var record3 = new Record(rdfString3);
        record.Should().NotBe(record3);
    }

    public static string RandomRecord(string id = "0", int numberScopes = 1, int numberDescribes = 1)
    {
        var iri = $"https://ssi.example.com/record/{id}";
        var start = $"<{iri}> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/record/Record> <{iri}> .";

        var scopes = "";
        var scopeTemplate = "<{0}> <https://rdf.equinor.com/ontology/record/isInScope> <https://ssi.example.com/scope/{1}> <{2}> .";
        for (var i = 0; i < numberScopes; i++) scopes += string.Format(scopeTemplate, iri, i, iri) + '\n';

        var descs = "";
        var descTemplate = "<{0}> <https://rdf.equinor.com/ontology/record/describes> <https://ssi.example.com/described/{1}> <{2}> .";
        for (var i = 0; i < numberDescribes; i++) descs += string.Format(descTemplate, iri, i, iri) + '\n';

        var replaces = $"\n<{iri}> <{Namespaces.Record.Replaces}> <https://ssi.example.com/record/0> <{iri}> .";
        return start + scopes + descs + replaces;
    }

    [Fact]
    public void Record_Can_Write_To_JsonLd()
    {
        var record = new Record(rdf);
        var jsonLdString = record.ToString<JsonLdRecordWriter>();

        var jsonObject = default(JsonObject);

        var deserialisationFunc = () => jsonObject = JsonSerializer.Deserialize<JsonArray>(jsonLdString).First() as JsonObject;
        deserialisationFunc.Should().NotThrow();

        jsonObject.Should().NotBeNull();
        jsonObject?.ContainsKey("@id").Should().BeTrue();

        var jsonObjectId = jsonObject?["@id"]?.GetValue<string>();
        jsonObjectId.Should().Be("https://akersolutions.com/data/RecordID123");
    }

    [Fact]
    public void Record_Can_Be_SubRecord()
    {
        var record = default(Record);
        var loadResult = () => record = new Record(rdf4);
        loadResult.Should().NotThrow();

        record.Should().NotBeNull();

        record.IsSubRecordOf.Should().NotBeNull();
        record.IsSubRecordOf.Should().Be("http://ssi.example.com/record/original");
    }

    [Fact]
    public void Record_Does_Not_Need_SubRecordOf()
    {
        var record = default(Record);
        var loadResult = () => record = new Record(rdf3);
        loadResult.Should().NotThrow();

        record.Should().NotBeNull();
        record.IsSubRecordOf.Should().BeNull();
    }

    [Fact]
    public void Record_Can_Have_At_Most_One_SuperRecord()
    {
        var record = default(Record);
        var loadResult = () => record = new Record(rdf5);
        loadResult.Should()
            .Throw<RecordException>()
            .WithMessage("Failure in record. A record can at most be the subrecord of one other record.");

        record.Should().BeNull();
    }
}

