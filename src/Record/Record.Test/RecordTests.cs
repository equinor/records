using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using FluentAssertions;
using Record = Records.Immutable.Record;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Writing;

namespace Records.Tests;

public class RecordTests
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
        var (s, p, o, g) = TestData.CreateRecordQuad("1");
        var rdf = $"<{s}> <{p}> <{o}> <{g}> .";

        var result = () => new Record(rdf);
        result.Should().Throw<RecordException>().WithMessage("Failure in record. A record must have exactly one provenance object.");
    }

    [Fact]
    public void Record_Can_Be_Serialised_Nquad()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());

        var result = record.ToString<NQuadsRecordWriter>().Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(22);
    }

    [Fact]
    public void Record_Can_Be_Serialised_Turtle()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());

        var result = record.ToString<TurtleWriter>().Split("\n").Length;

        // This is how many lines should be contained in the turtle serialisation
        result.Should().Be(28);
    }

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
        var record = new Record(TestData.ValidNQuadRecordString());
        var jsonLdString = record.ToString<JsonLdRecordWriter>();

        var jsonObject = default(JsonObject);

        var deserialisationFunc = () => jsonObject = JsonSerializer.Deserialize<JsonArray>(jsonLdString).First() as JsonObject;
        deserialisationFunc.Should().NotThrow();

        jsonObject.Should().NotBeNull();
        jsonObject?.ContainsKey("@id").Should().BeTrue();

        var jsonObjectId = jsonObject?["@id"]?.GetValue<string>();
        jsonObjectId.Should().Be("https://ssi.example.com/record/1");
    }
}

