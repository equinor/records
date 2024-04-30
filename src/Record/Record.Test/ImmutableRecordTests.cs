using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Record = Records.Immutable.Record;
using Records.Exceptions;
using VDS.RDF.Writing;
using Newtonsoft.Json;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Records.Tests;

public class ImmutableRecordTests
{
    [Fact]
    public void Record_Has_Provenance()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var result = record.Provenance.Count();

        result.Should().Be(13);
    }

    [Fact]
    public void Record_Finds_Id()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var result = record.Id;

        result.Should().Be("https://ssi.example.com/record/1");
    }

    [Fact]
    public void Record_CanBeCreated_FromGraph()
    {
        ITripleStore store = new TripleStore();
        store.LoadFromString(TestData.ValidJsonLdRecordString(), new JsonLdParser());

        var record = new Record(store.Graphs.First());
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
        result.Should().Be(28);
    }

    [Fact]
    public void Record_Can_Be_Serialised_Nquad_With_Direct_Writer()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());

        var result = record.ToString(new NQuadsWriter()).Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(28);
    }



    [Fact]
    public void Record_Can_Produce_Quads()
    {
        var record = new Record(TestData.ValidJsonLdRecordString());
        var result = record.Quads().Count();

        // This is how many quads should be extraced from the JSON-LD
        result.Should().Be(27);
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

    [Fact]
    public void Record_Content_Does_Not_Include_Provenance()
    {
        var record = default(Record);
        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };
        loadResult.Should().NotThrow();

        var provenance = record.ProvenanceAsTriples();
        var content = record.ContentAsTriples();
        var contentSubjects = content.Select(t => t.Subject.ToString()).ToList();
        contentSubjects.Should().NotContain(record.Id);
        content.Should().NotContain(provenance);
    }

    [Fact]
    public void Record_Provenance_Only_Contains_Provenance()
    {
        var record = default(Record);
        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };
        loadResult.Should().NotThrow();

        var provenance = record.ProvenanceAsTriples();
        var provenanceSubjects = provenance.Select(t => t.Subject.ToString());
        var provenanceSubjectsHashSet = provenanceSubjects.ToHashSet();
        provenanceSubjectsHashSet.Count.Should().Be(1, "The only subject should be the record ID.");
        provenanceSubjectsHashSet.Single().Should().Be(record.Id, "The subject in the provenance should be the record ID.");
    }

    [Fact]
    public void Record_Can_Copy_Of_Internal_Graph()
    {
        var record = default(Record);
        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };

        loadResult.Should().NotThrow();

        var graph = record.Graph();
        graph.Name.ToString().Should().Be(record.Id);
        record.Triples().Should().Contain(graph.Triples);

        graph.Clear();

        graph.IsEmpty.Should().BeTrue();

        record.Graph().IsEmpty.Should().BeFalse();
    }

    [Fact]
    public void Record_Can_Query_Subjects_With_Type()
    {
        var record = default(Record);

        var typeExample = new UriNode(new Uri("https://example.com/type/Cool"));
        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));
        var content = new Triple(subjectExample, new UriNode(new Uri(Namespaces.Rdf.Type)), typeExample);

        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(content)
            .Build();
        };

        loadResult.Should().NotThrow();

        var subjects = record.SubjectWithType(typeExample);

        subjects.Count().Should().Be(1);
        subjects.Single().Should().Be(subjectExample);
    }

    [Fact]
    public void Record_Can_Query_Subjects_With_Label()
    {
        var record = default(Record);

        var labelExample = "This is an example label.";
        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));
        var content = new Triple(subjectExample, new UriNode(new Uri(Namespaces.Rdfs.Label)), new LiteralNode(labelExample));

        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(content)
            .Build();
        };

        loadResult.Should().NotThrow();

        var labels = record.LabelsOfSubject(subjectExample);

        labels.Count().Should().Be(1);
        labels.Single().Should().Be(labelExample);
    }

    [Fact]
    public void Record_Can_Query_Graphs()
    {
        var record = default(Record);

        var recordId = TestData.CreateRecordId(1);

        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));

        var labelExample = "This is an example label.";
        var labelTriple = new Triple(subjectExample, new UriNode(new Uri(Namespaces.Rdfs.Label)), new LiteralNode(labelExample));

        var typeExample = new UriNode(new Uri("https://example.com/type/Example"));
        var typeTriple = new Triple(subjectExample, new UriNode(new Uri(Namespaces.Rdf.Type)), typeExample);

        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete(recordId)
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(labelTriple)
            .WithAdditionalContent(typeTriple)
            .Build();
        };

        loadResult.Should().NotThrow();

        var labelTriples = record.QuadsWithPredicateAndObject(new UriNode(new Uri(Namespaces.Rdfs.Label)), new LiteralNode(labelExample));

        labelTriples.Count().Should().Be(1);
        labelTriples.Single().Should().Be(Quad.CreateUnsafe(labelTriple, recordId));


        var typeTriples = record.QuadsWithSubjectPredicate(subjectExample, new UriNode(new Uri(Namespaces.Rdf.Type)));

        typeTriples.Count().Should().Be(1);
        typeTriples.Single().Should().Be(Quad.CreateUnsafe(typeTriple, recordId));


        var recordTriples = record.QuadsWithSubjectObject(new UriNode(new Uri(recordId)), new UriNode(new Uri(Namespaces.Record.RecordType)));

        recordTriples.Count().Should().Be(1);
        recordTriples.Single().Should().Be(Quad.CreateUnsafe(recordId, Namespaces.Rdf.Type, Namespaces.Record.RecordType, recordId));
    }
}

