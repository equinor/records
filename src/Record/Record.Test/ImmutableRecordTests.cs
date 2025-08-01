﻿using System.Text.Json.Nodes;
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
    public void Record_Has_Metadata()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);
        var result = record.Metadata!.Count();

        result.Should().Be(14);
    }

    [Fact]
    public void Record_Finds_Id()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);
        var result = record.Id;

        result.Should().Be("https://ssi.example.com/record/1");
    }

    [Fact]
    public void Record_CanBeCreated_FromGraph()
    {
        ITripleStore store = new TripleStore();
        var graph = TestData.ValidRecord().GetMergedGraphs();

        var record = new Record(graph, ignoreDescribesConstraint: true);
        var result = record.Id;

        result.Should().Be("https://ssi.example.com/record/1");
    }

    [Fact]
    public void Record_Can_Do_Queries()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);
        var queryResult = record.Sparql($"construct {{ ?s ?p ?o }} where {{ graph ?g {{ ?s ?p ?o . ?s <{Namespaces.Record.IsInScope}> ?o .}} }}");
        var result = queryResult.Count();
        result.Should().Be(5);
    }

    [Fact]
    public void Record_Does_Not_Have_Metadata()
    {
        var (s, p, o, g) = TestData.CreateRecordQuadStringTuple("1");
        var rdf = $"<{s}> <{p}> <{o}> <{g}> .";

        var result = () => new Record(rdf);

        result.Should().Throw<RecordException>().WithMessage("A record must have exactly one metadata graph.");
    }


    [Fact]
    public void Creating_Record_From_Invalid_JsonLD_Throws()
    {
        var invalidJsonLdString = TestData.ValidJsonLdRecordString() + TestData.ValidJsonLdRecordString();
        var result = () => new Record(invalidJsonLdString);
        result.Should().Throw<RecordException>().WithInnerException<JsonReaderException>();
    }

    [Fact]
    public void Record_Can_Be_Serialised_Nquad()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);

        var result = record.ToString<NQuadsWriter>().Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(32);
    }

    [Fact]
    public void Record_Can_Be_Serialised_Nquad_With_Direct_Writer()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);

        var result = record.ToString(new NQuadsWriter()).Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(32);
    }



    [Fact]
    public void Record_Can_Produce_Quads()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);
        var result = record.Triples().Count();

        // This is how many quads should be extraced from the JSON-LD
        result.Should().Be(31);
    }

    [Fact]
    public void Record_Has_Scopes_And_Describes()
    {
        var record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);

        var scopes = record.Scopes;
        var describes = record.Describes;

        var scopeCount = scopes!.Count();
        var describesCount = describes!.Count();

        scopeCount.Should().Be(5);
        describesCount.Should().Be(5);
    }

    [Fact]
    public void Record_With_Same_Scopes_And_Describes_Are_Equal()
    {
        var rdfString1 = TestData.ValidNQuadRecordString(TestData.CreateRecordId("1"));
        var rdfString2 = TestData.ValidNQuadRecordString(TestData.CreateRecordId("2"));

        var record = new Record(rdfString1, ignoreDescribesConstraint: true);
        var record2 = new Record(rdfString2, ignoreDescribesConstraint: true);

        record.Should().Be(record2);
    }

    [Fact]
    public void Record_With_Different_Scopes_And_Describes_Are_Not_Equal()
    {
        var id1 = TestData.CreateRecordId("1");
        var id2 = TestData.CreateRecordId("2");

        var rdfString1 = TestData.ValidNQuadRecordString(id1, 3, 2);
        var rdfString2 = TestData.ValidNQuadRecordString(id2, 3, 2);

        var record = new Record(rdfString1, ignoreDescribesConstraint: true);
        var record2 = new Record(rdfString2, ignoreDescribesConstraint: true);

        record.Should().Be(record2);

        var id3 = TestData.CreateRecordId("3");
        var rdfString3 = TestData.ValidNQuadRecordString(id3, 2, 2);

        var record3 = new Record(rdfString3, ignoreDescribesConstraint: true);
        record.Should().NotBe(record3);
    }

    [Fact]
    public void Record_Can_Write_To_JsonLd()
    {
        var record = new Record(TestData.ValidNQuadRecordString(), ignoreDescribesConstraint: true);

        var jsonLdString = record.ToString<JsonLdWriter>();

        var jsonArray = default(JsonArray);

        var deserialisationFunc = () => jsonArray = JsonNode.Parse(jsonLdString) as JsonArray;
        deserialisationFunc.Should().NotThrow();

        jsonArray.Should().NotBeNull();
        jsonArray?.Count.Should().Be(2);

        jsonArray!
            .Any(child => (child as JsonObject)!["@id"]!.ToString()!.Equals("https://ssi.example.com/record/1"))
            .Should().BeTrue();
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

        record!.IsSubRecordOf.Should().NotBeNull();
        record.IsSubRecordOf.Should().Be(superRecordId);
    }

    [Fact]
    public void Record_Does_Not_Need_SubRecordOf()
    {
        var record = default(Record);
        var loadResult = () => record = new Record(TestData.ValidJsonLdRecordString(), ignoreDescribesConstraint: true);
        loadResult.Should().NotThrow();

        record.Should().NotBeNull();
        record!.IsSubRecordOf.Should().BeNull();
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

            var recordString = immutable.ToString(new NQuadsWriter());
            recordString += $"<{immutable.Id}> <{Namespaces.Record.IsSubRecordOf}> <{TestData.CreateRecordId("supersuper")}> .\n";

            record = new Record(recordString);
        };
        loadResult.Should()
            .Throw<RecordException>()
            .WithMessage("A record can at most be the subrecord of one other record.");

        record.Should().BeNull();
    }

    [Fact]
    public void Record_Content_Does_Not_Include_Metadata()
    {
        var record = default(Record);
        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };
        loadResult.Should().NotThrow();

        var metadata = record!.MetadataAsTriples();
        var content = record.ContentAsTriples();
        var contentSubjects = content.Select(t => t.Subject.ToString()).ToList();
        contentSubjects.Should().NotContain(record.Id);
        content.Should().NotContain(metadata);
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

        var tripleStore = record!.TripleStore();
        record.Triples().Should().Contain(tripleStore.Triples);
    }

    [Fact]
    public void Record_Can_Be_Copied_To_New_Record_Via_ITripleStore()
    {
        var record = default(Record);
        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };

        loadResult.Should().NotThrow();

        var tripleStore = record!.TripleStore();
        var metadataGraph = record.MetadataGraph();
        metadataGraph.Name.ToString().Should().Be(record.Id);

        record.Triples().Should().Contain(tripleStore.Triples);

        var newRecord = new Record(tripleStore, ignoreDescribesConstraint: true);
        newRecord.Should().Be(record);
        newRecord.Id.Should().Be(record.Id);
        newRecord.Triples().Should().Contain(record.Triples());
    }

    [Fact]
    public void Record_Can_Query_Subjects_With_Type()
    {
        var record = default(Record);

        var typeExample = new UriNode(new Uri("https://example.com/type/Cool"));
        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));
        var content = new Triple(subjectExample, Namespaces.Rdf.UriNodes.Type, typeExample);

        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(content)
            .Build();
        };

        loadResult.Should().NotThrow();

        var subjects = record!.SubjectWithType(typeExample);

        subjects.Count().Should().Be(1);
        subjects.Single().Should().Be(subjectExample);
    }

    [Fact]
    public void Record_Can_Query_Subjects_With_Label()
    {
        var record = default(Record);

        var labelExample = "This is an example label.";
        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));
        var content = new Triple(subjectExample, Namespaces.Rdfs.UriNodes.Label, new LiteralNode(labelExample));

        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(content)
            .Build();
        };

        loadResult.Should().NotThrow();

        var labels = record!.LabelsOfSubject(subjectExample);

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
        var labelTriple = new Triple(subjectExample, Namespaces.Rdfs.UriNodes.Label, new LiteralNode(labelExample));

        var typeExample = new UriNode(new Uri("https://example.com/type/Example"));
        var typeTriple = new Triple(subjectExample, Namespaces.Rdf.UriNodes.Type, typeExample);

        var loadResult = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete(recordId)
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(labelTriple)
            .WithAdditionalContent(typeTriple)
            .Build();
        };

        loadResult.Should().NotThrow();

        var labelTriples = record!.TriplesWithPredicateAndObject(Namespaces.Rdfs.UriNodes.Label, new LiteralNode(labelExample));

        labelTriples.Count().Should().Be(1);
        labelTriples.Single().Should().Be(labelTriple);


        var typeTriples = record.TriplesWithSubjectPredicate(subjectExample, Namespaces.Rdf.UriNodes.Type);

        typeTriples.Count().Should().Be(1);
        typeTriples.Single().Should().Be(typeTriple);


        var recordTriples = record.TriplesWithSubjectObject(new UriNode(new Uri(recordId)), Namespaces.Record.UriNodes.RecordType);

        recordTriples.Count().Should().Be(1);
        recordTriples.Single().Should().Be(new Triple(new UriNode(new Uri(recordId)), Namespaces.Rdf.UriNodes.Type, Namespaces.Record.UriNodes.RecordType));
    }

    [Fact]
    public void Record_Can_Be_Created_From_String_And_IStoreReader()
    {
        var recordString = TestData.ValidJsonLdRecordString();
        var reader = new JsonLdParser();

        var loadResult = () =>
        {
            var record = new Record(recordString, reader, ignoreDescribesConstraint: true);
        };

        loadResult.Should().NotThrow();
    }

    [Fact]
    public void Record_With_Wrong_IStoreReader_Fails_To_Load()
    {
        var recordString = TestData.ValidJsonLdRecordString();
        var reader = new NQuadsParser();

        var loadResult = () =>
        {
            var record = new Record(recordString, reader);
        };

        loadResult.Should().Throw<RdfParseException>();
    }

    [Fact]
    public void Record_Can_Load_From_TripleStore()
    {
        var originalRecord = TestData.ValidRecord();

        var recordString = originalRecord.ToString<JsonLdWriter>();
        var store = new TripleStore();
        store.LoadFromString(recordString, new JsonLdParser());

        var record = default(Record);

        var loadResult = () =>
        {
            record = new Record(store, ignoreDescribesConstraint: true);
        };

        loadResult.Should().NotThrow();

        record.Should().NotBeNull();
        record!.Id.Should().Be(originalRecord.Id);
    }

    [Fact]
    public void Record_Collapse_ShouldReturnSingleGraphWithAllTriples()
    {
        // Arrange
        var recordId = "https://example.com/1";

        var record = TestData.ValidRecord(TestData.CreateRecordId(recordId));
        var tripleStore = record.TripleStore();

        var collapsedGraph = tripleStore.Collapse(recordId);

        // Act
        var result = record.GetMergedGraphs();

        // Assert
        result.Should().BeEquivalentTo(collapsedGraph);
        result.Triples.Should().HaveCount(tripleStore.Triples.Count());
        tripleStore.Triples.Should().Contain(tripleStore.Triples);
    }

    [Fact]
    public void RecordGraph_Can_Create_New_Record()
    {
        // Arrange
        var recordId = "https://example.com/1";

        var record = TestData.ValidRecord(recordId);
        var graph = record.GetMergedGraphs();

        // Act
        var result = new Record(graph, ignoreDescribesConstraint: true);

        // Assert
        result.Should().Be(record);
        result.Triples().Count().Should().Be(record.Triples().Count());
    }

    [Fact]
    public void Record_GetContentGraphs_Returns_All_Content_Graphs()
    {
        // Arrange
        var recordId = "https://example.com/1";

        var record = TestData.ValidRecord(recordId);

        // Act
        var result = record.GetContentGraphs();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().NotBe(recordId);
    }
}