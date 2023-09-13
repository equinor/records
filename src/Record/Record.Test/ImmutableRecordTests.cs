using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Record = Records.Immutable.Record;
using Records.Exceptions;
using VDS.RDF.Writing;
using Newtonsoft.Json;

namespace Records.Tests;

public class ImmutableRecordTests
{
    [Fact]
    public void Record_Has_Provenance()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var result = record.Provenance.Count();

        result.Should().Be(11);
    }

    [Fact]
    public void Record_Finds_Id()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var result = record.Id;

        result.Should().Be("https://ssi.example.com/record/1");
    }

    [Fact]
    public void Record_Can_Do_Queries()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var queryResult = record.Sparql($"construct {{ ?s ?p ?o }} where {{ ?s ?p ?o . ?s <{Namespaces.Record.IsInScope}> ?o .}}");
        var result = queryResult.Count();
        result.Should().Be(5);
    }

    [Fact]
    public void Record_Does_Not_Have_Provenance()
    {
        var (s, p, o, g) = TestData.CreateRecordQuadStringTuple("1");
        var rdf = $"<{s}> <{p}> <{o}> <{g}> .";

        var result = () => new Record(rdf);

        result.Should().Throw<RecordException>().WithMessage("A record must have exactly one provenance object.");
    }


    [Fact]
    public void Creating_Record_From_Invalid_JsonLD_Throws()
    {
        var invalidJsonLdString = TestData.ValidJsonLdRecordString() + TestData.ValidJsonLdRecordString();
        var result = () => new Record(invalidJsonLdString);
        result.Should().Throw<RecordException>().WithInnerException<JsonReaderException>();
    }


    [Fact]
    public void Creating_Record_With_More_Than_One_Named_Graph_Throws()
    {
        var jsonArray = $"[{TestData.ValidJsonLdRecordString(TestData.CreateRecordId(1))}," +
                        $"{TestData.ValidJsonLdRecordString(TestData.CreateRecordId(2))}]";

        var result = () => new Record(jsonArray);
        result.Should().Throw<RecordException>().WithMessage("A record must contain exactly one named graph.");
    }


    [Fact]
    public void Record_Can_Be_Serialised_Nquad()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());

        var result = record.ToString<NQuadsWriter>().Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(22);
    }

    //[Fact]
    //public void Record_Can_Be_Serialised_Turtle()
    //{
    //    var record = new Record(TestData.ValidJsonLdRecordString());

    //    var result = record.ToString<CompressingTurtleWriter>().Split("\n").Length;

    //    // This is how many lines should be contained in the turtle serialisation
    //    result.Should().Be(28);
    //}

    [Fact]
    public void Record_Can_Produce_Quads()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var result = record.Quads().Count();

        // This is how many quads should be extraced from the JSON-LD
        result.Should().Be(21);
    }

    [Fact]
    public void Record_Has_Scopes_And_Describes()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());

        var scopes = record.Scopes;
        var describes = record.Describes;

        var scopeCount = scopes.Count();
        var describesCount = describes.Count();

        scopeCount.Should().Be(5);
        describesCount.Should().Be(5);
    }

    [Fact]
    public void Record_With_Same_Scopes_And_Describes_Are_Equal()
    {
        var rdfString1 = TestData.ValidNQuadRecordString(TestData.CreateRecordId("1"));
        var rdfString2 = TestData.ValidNQuadRecordString(TestData.CreateRecordId("2"));

        var record = new Record(rdfString1);
        var record2 = new Record(rdfString2);

        record.Should().Be(record2);
    }

    [Fact]
    public void Record_With_Different_Scopes_And_Describes_Are_Not_Equal()
    {
        var id1 = TestData.CreateRecordId("1");
        var id2 = TestData.CreateRecordId("2");

        var rdfString1 = TestData.ValidNQuadRecordString(id1, 3, 2);
        var rdfString2 = TestData.ValidNQuadRecordString(id2, 3, 2);

        var record = new Record(rdfString1);
        var record2 = new Record(rdfString2);

        record.Should().Be(record2);

        var id3 = TestData.CreateRecordId("3");
        var rdfString3 = TestData.ValidNQuadRecordString(id3, 2, 2);

        var record3 = new Record(rdfString3);
        record.Should().NotBe(record3);
    }

    [Fact]
    public void Record_Can_Write_To_JsonLd()
    {
        var record = new Record(TestData.ValidNQuadRecordString());

        var jsonLdString = record.ToString<JsonLdWriter>();

        var jsonObject = default(JsonObject);

        var deserialisationFunc = () => jsonObject = System.Text.Json.JsonSerializer.Deserialize<JsonObject>(jsonLdString);
        deserialisationFunc.Should().NotThrow();

        jsonObject.Should().NotBeNull();
        jsonObject?.ContainsKey("@id").Should().BeTrue();

        var jsonObjectId = jsonObject?["@id"]?.GetValue<string>();
        jsonObjectId.Should().Be("https://ssi.example.com/record/1");
    }

    [Fact]
    public void Record_Can_Be_SubRecord()
    {
        var record = default(Record);
        var superRecordId = TestData.CreateRecordId("super");
        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
                .WithIsSubRecordOf(superRecordId)
                .Build();
        };
        loadResult.Should().NotThrow();

        record.Should().NotBeNull();

        record.IsSubRecordOf.Should().NotBeNull();
        record.IsSubRecordOf.Should().Be(superRecordId);
    }

    [Fact]
    public void Record_Does_Not_Need_SubRecordOf()
    {
        var record = default(Record);
        var loadResult = () => record = new Record(TestData.ValidJsonLdRecordString());
        loadResult.Should().NotThrow();

        record.Should().NotBeNull();
        record.IsSubRecordOf.Should().BeNull();
    }

    [Fact]
    public void Record_Can_Have_At_Most_One_SuperRecord()
    {
        var record = default(Record);
        var loadResult = () =>
        {
            var immutable = TestData.ValidRecordBeforeBuildComplete()
                .WithIsSubRecordOf(TestData.CreateRecordId("super"))
                .Build();
            var mutable = new Mutable.Record(immutable).WithIsSubRecordof(TestData.CreateRecordId("superduper"));
            record = mutable.ToImmutable();
        };
        loadResult.Should()
            .Throw<RecordException>()
            .WithMessage("A record can at most be the subrecord of one other record.");

        record.Should().BeNull();
    }

}

