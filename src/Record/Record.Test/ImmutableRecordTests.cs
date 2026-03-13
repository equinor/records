using System.Text.Json.Nodes;
using FluentAssertions;
using Records.Exceptions;
using VDS.RDF.Writing;
using Record.Test.TestInfrastructure;
using Records.Backend;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Records.Tests;

[Collection("Integration Testing Collection")]
public class ImmutableRecordTests(FusekiContainerManager fusekiContainerManager)
{
    readonly Uri _connectionUri = fusekiContainerManager.address;
    public enum BackendType
    {
        DotNetRdf,
        Fuseki
    }
    private async Task<IRecordBackend> CreateBackend(BackendType backendType, RdfMediaType mediaType, string rdfstring)
    {
        HttpClient httpClient = new HttpClient() { BaseAddress = _connectionUri };
        IRecordBackend backend = backendType switch
        {

            BackendType.Fuseki => await FusekiRecordBackend.CreateAsync(rdfstring, mediaType, httpClient),
            BackendType.DotNetRdf => new DotNetRdfRecordBackend(rdfstring),
            _ => throw new ArgumentException("Invalid backend type")
        };
        return backend;
    }


    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Has_Metadata(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));
        var result = record.Metadata!.Count();

        result.Should().Be(21);
    }


    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Add_Metadata(BackendType backendType)
    {
        // Arrange
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));
        var oldRecordMetadataCount = record.Metadata!.Count();
        var g = new Graph();
        g.Assert(new Triple(new UriNode(new Uri(record.Id)), new UriNode(new Uri("https://rdf.equinor.com/ontology/record/replaces")), new UriNode(new Uri(TestData.CreateRecordId("2")))));
        
        // Act
        var newRecord = await record.WithAdditionalMetadata(g);
        
        // Assert
        newRecord.Replaces.Should().Contain(TestData.CreateRecordId("2"));
        newRecord.Replaces.Count().Should().Be(1);
        record.Replaces!.Count().Should().Be(0);
        var oldRecordMetadataCountAfterAdditional = record.Metadata!.Count();
        var newRecordMetadataCount = newRecord.Metadata!.Count();
        oldRecordMetadataCount.Should().Be(21);
        oldRecordMetadataCountAfterAdditional.Should().Be(21);
        newRecordMetadataCount.Should().Be(22);
    }
    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Finds_Id(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));
        var result = record.Id;

        result.Should().Be("https://ssi.example.com/record/1");
    }

    [Fact]
    public async Task Record_CanBeCreated_FromGraph()
    {
        ITripleStore store = new TripleStore();
        var graph = await (await TestData.ValidRecord()).GetMergedGraphs();

        var record = await Immutable.Record.CreateAsync(graph);
        var result = record.Id;

        result.Should().Be("https://ssi.example.com/record/1");
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Do_Queries(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));
        var queryResult = await record.Sparql(
            $"construct {{ ?s ?p ?o }} where {{ " +
            $"graph ?g {{ " +
            $"?s ?p ?o . " +
            $"?s <{Namespaces.Record.IsInScope}> ?o ." +
            $"}} }}");
        var result = queryResult.Count();
        result.Should().Be(5);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Does_Not_Have_Metadata(BackendType backendType)
    {
        var (s, p, o, g) = TestData.CreateRecordQuadStringTuple("1");
        var rdf = $"<{s}> <{p}> <{o}> <{g}> .";

        var result = async () => await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, rdf));

        await result.Should().ThrowAsync<RecordException>().WithMessage("Could not find a metadata graph. A record must have exactly one metadata graph");
    }


    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Creating_Record_From_Invalid_JsonLD_Throws(BackendType backendType)
    {
        var invalidJsonLdString = await TestData.ValidJsonLdRecordString() + TestData.ValidJsonLdRecordString();
        var result = async () => await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, invalidJsonLdString));
        await result.Should().ThrowAsync<RecordException>();
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Be_Serialised_Nquad(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));

        var recordString = await record.ToString<NQuadsWriter>();
        var result = recordString.Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(32);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Be_Serialised_Nquad_With_Direct_Writer(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));

        var result = (await record.ToString(new NQuadsWriter())).Split("\n").Length;

        // This is how many quads are generated
        result.Should().Be(32);
    }



    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Produce_Quads(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));
        var result = (await record.Triples()).Count();

        // This is how many quads should be extraced from the JSON-LD
        result.Should().Be(31);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Has_Scopes_And_Describes(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));

        var scopes = record.Scopes;
        var describes = record.Describes;

        var scopeCount = scopes!.Count();
        var describesCount = describes!.Count();

        scopeCount.Should().Be(5);
        describesCount.Should().Be(5);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_With_Same_Scopes_And_Describes_Are_Equal(BackendType backendType)
    {
        var rdfString1 = await TestData.ValidNQuadRecordString(TestData.CreateRecordId("1"));
        var rdfString2 = await TestData.ValidNQuadRecordString(TestData.CreateRecordId("2"));

        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, rdfString1));
        var record2 = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, rdfString2));

        record.Should().Be(record2);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_With_Different_Scopes_And_Describes_Are_Not_Equal(BackendType backendType)
    {
        var id1 = TestData.CreateRecordId("1");
        var id2 = TestData.CreateRecordId("2");

        var rdfString1 = await TestData.ValidNQuadRecordString(id1, 3, 2);
        var rdfString2 = await TestData.ValidNQuadRecordString(id2, 3, 2);

        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, rdfString1));
        var record2 = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, rdfString2));

        record.Should().Be(record2);

        var id3 = TestData.CreateRecordId("3");
        var rdfString3 = await TestData.ValidNQuadRecordString(id3, 2, 2);

        var record3 = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, rdfString3));
        record.Should().NotBe(record3);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Write_To_JsonLd(BackendType backendType)
    {
        var record = await Immutable.Record.CreateAsync(await CreateBackend(backendType,
            RdfMediaType.Quads,
            await TestData.ValidNQuadRecordString()));

        var jsonLdString = await record.ToString<JsonLdWriter>();
        var jsonObject = JsonNode.Parse(jsonLdString);
        JsonArray? jsonArray = jsonObject switch
        {
            JsonArray arr => arr,
            JsonObject obj => obj["@graph"] as JsonArray,
            _ => throw new Exception("Unexpected JSON structure")
        };

        jsonArray.Should().NotBeNull();
        jsonArray?.Count.Should().Be(2);

        jsonArray!
            .Any(child => (child as JsonObject)!["@id"]!.ToString()!.Equals("https://ssi.example.com/record/1"))
            .Should().BeTrue();
    }

    [Fact]
    public async Task Record_Can_Be_SubRecord()
    {
        var record = default(Immutable.Record);
        var superRecordId = TestData.CreateRecordId("super");
        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete()
                .WithIsSubRecordOf(superRecordId)
                .Build();
        };
        await loadResult.Should().NotThrowAsync();

        record.Should().NotBeNull();

        record!.IsSubRecordOf.Should().NotBeNull();
        record.IsSubRecordOf.Should().Be(superRecordId);
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Does_Not_Need_SubRecordOf(BackendType backendType)
    {
        var record = default(Immutable.Record);
        var loadResult = async () => record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.JsonLd, await TestData.ValidJsonLdRecordString()));
        await loadResult.Should().NotThrowAsync();

        record.Should().NotBeNull();
        record!.IsSubRecordOf.Should().BeNull();
    }

    [Theory]
    [InlineData(BackendType.DotNetRdf)]
    [InlineData(BackendType.Fuseki)]
    public async Task Record_Can_Have_At_Most_One_SuperRecord(BackendType backendType)
    {
        var record = default(Immutable.Record);
        var loadResult = async () =>
        {
            var immutable = await TestData.ValidRecordBeforeBuildComplete()
                .WithIsSubRecordOf(TestData.CreateRecordId("super"))
                .Build();

            var recordString = await immutable.ToString(new NQuadsWriter());
            recordString += $"<{immutable.Id}> <{Namespaces.Record.IsSubRecordOf}> <{TestData.CreateRecordId("supersuper")}>  <{immutable.Id}> .\n";

            record = await Immutable.Record.CreateAsync(await CreateBackend(backendType, RdfMediaType.Quads, recordString));
        };
        await loadResult.Should()
            .ThrowAsync<RecordException>()
            .WithMessage("A record can at most be the subrecord of one other record.");

        record.Should().BeNull();
    }

    [Fact]
    public async Task Record_Content_Does_Not_Include_Metadata()
    {
        var record = default(Immutable.Record);
        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };
        await loadResult.Should().NotThrowAsync();

        var metadata = await record!.MetadataAsTriples();
        var content = await record.ContentAsTriples();
        var contentSubjects = content.Select(t => t.Subject.ToString()).ToList();
        contentSubjects.Should().NotContain(record.Id);
        content.Should().NotContain(metadata);
    }

    [Fact]
    public async Task Record_Can_Copy_Of_Internal_Graph()
    {
        var record = default(Immutable.Record);
        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };

        await loadResult.Should().NotThrowAsync();

        var tripleStore = await record!.TripleStore();
        (await record.Triples()).Should().Contain(tripleStore.Triples);
    }

    [Fact]
    public async Task Record_Can_Be_Copied_To_New_Record_Via_ITripleStore()
    {
        var record = default(Immutable.Record);
        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .Build();
        };

        await loadResult.Should().NotThrowAsync();

        var tripleStore = await record!.TripleStore();
        var metadataGraph = await record.MetadataGraph();
        metadataGraph.Name.ToString().Should().Be(record.Id);

        (await record.Triples()).Should().Contain(tripleStore.Triples);

        var newRecord = await Immutable.Record.CreateAsync(tripleStore);
        newRecord.Should().Be(record);
        newRecord.Id.Should().Be(record.Id);
        (await newRecord.Triples()).Should().Contain(await record.Triples());
    }

    [Fact]
    public async Task Record_Can_Query_Subjects_With_Type()
    {
        var record = default(Immutable.Record);

        var typeExample = new UriNode(new Uri("https://example.com/type/Cool"));
        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));
        var content = new Triple(subjectExample, Namespaces.Rdf.UriNodes.Type, typeExample);

        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(content)
            .Build();
        };

        await loadResult.Should().NotThrowAsync();

        var subjects = await record!.SubjectWithType(typeExample);

        subjects.Count().Should().Be(1);
        subjects.Single().Should().Be(subjectExample);
    }

    [Fact]
    public async Task Record_Can_Query_Subjects_With_Label()
    {
        var record = default(Immutable.Record);

        var labelExample = "This is an example label.";
        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));
        var content = new Triple(subjectExample, Namespaces.Rdfs.UriNodes.Label, new LiteralNode(labelExample));

        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(content)
            .Build();
        };

        await loadResult.Should().NotThrowAsync();

        var labels = await record!.LabelsOfSubject(subjectExample);

        labels.Count().Should().Be(1);
        labels.Single().Should().Be(labelExample);
    }

    [Fact]
    public async Task Record_Can_Query_Graphs()
    {
        var record = default(Immutable.Record);

        var recordId = TestData.CreateRecordId(1);

        var subjectExample = new UriNode(new Uri("https://example.com/subject/123"));

        var labelExample = "This is an example label.";
        var labelTriple = new Triple(subjectExample, Namespaces.Rdfs.UriNodes.Label, new LiteralNode(labelExample));

        var typeExample = new UriNode(new Uri("https://example.com/type/Example"));
        var typeTriple = new Triple(subjectExample, Namespaces.Rdf.UriNodes.Type, typeExample);

        var loadResult = async () =>
        {
            record = await TestData.ValidRecordBeforeBuildComplete(recordId)
            .WithIsSubRecordOf(TestData.CreateRecordId(1))
            .WithAdditionalContent(labelTriple)
            .WithAdditionalContent(typeTriple)
            .Build();
        };

        await loadResult.Should().NotThrowAsync();

        var labelTriples = await record!.TriplesWithPredicateAndObject(Namespaces.Rdfs.UriNodes.Label, new LiteralNode(labelExample));

        labelTriples.Count().Should().Be(1);
        labelTriples.Single().Should().Be(labelTriple);


        var typeTriples = await record.TriplesWithSubjectPredicate(subjectExample, Namespaces.Rdf.UriNodes.Type);

        typeTriples.Count().Should().Be(1);
        typeTriples.Single().Should().Be(typeTriple);


        var recordTriples = await record.TriplesWithSubjectObject(new UriNode(new Uri(recordId)), Namespaces.Record.UriNodes.RecordType);

        recordTriples.Count().Should().Be(1);
        recordTriples.Single().Should().Be(new Triple(new UriNode(new Uri(recordId)), Namespaces.Rdf.UriNodes.Type, Namespaces.Record.UriNodes.RecordType));
    }

    [Fact]
    public async Task Record_Can_Be_Created_From_String_And_IStoreReader()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var reader = new JsonLdParser();

        var loadResult = async () =>
        {
            var record = await Immutable.Record.CreateAsync(recordString, reader);
        };

        await loadResult.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Record_With_Wrong_IStoreReader_Fails_To_Load()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var reader = new NQuadsParser();

        var loadResult = async () =>
        {
            var record = await Immutable.Record.CreateAsync(recordString, reader);
        };

        await loadResult.Should().ThrowAsync<RdfParseException>();
    }

    [Fact]
    public async Task Record_Can_Load_From_TripleStore()
    {
        var originalRecord = await TestData.ValidRecord();

        var recordString = await originalRecord.ToString<JsonLdWriter>();
        var store = new TripleStore();
        store.LoadFromString(recordString, new JsonLdParser());

        var record = default(Immutable.Record);

        var loadResult = async () =>
        {
            record = await Immutable.Record.CreateAsync(store);
        };

        await loadResult.Should().NotThrowAsync();

        record.Should().NotBeNull();
        record!.Id.Should().Be(originalRecord.Id);
    }

    [Fact]
    public async Task Record_Collapse_ShouldReturnSingleGraphWithAllTriples()
    {
        // Arrange
        var recordId = "https://example.com/1";

        var record = await TestData.ValidRecord(TestData.CreateRecordId(recordId));
        var tripleStore = await record.TripleStore();

        var collapsedGraph = tripleStore.Collapse(recordId);

        // Act
        var result = await record.GetMergedGraphs();

        // Assert
        result.Should().BeEquivalentTo(collapsedGraph);
        result.Triples.Should().HaveCount(tripleStore.Triples.Count());
        tripleStore.Triples.Should().Contain(tripleStore.Triples);
    }

    [Fact]
    public async Task RecordGraph_Can_Create_New_Record()
    {
        // Arrange
        var recordId = "https://example.com/1";

        var record = await TestData.ValidRecord(recordId);
        var graph = await record.GetMergedGraphs();

        // Act
        var result = await Immutable.Record.CreateAsync(graph);

        // Assert
        result.Should().Be(record);
        (await result.Triples()).Count().Should().Be((await record.Triples()).Count());
    }

    [Fact]
    public async Task Record_GetContentGraphs_Returns_All_Content_Graphs()
    {
        // Arrange
        var recordId = "https://example.com/1";

        var record = await TestData.ValidRecord(recordId);

        // Act
        var result = await record.GetContentGraphs();

        // Assert
        result.Should().HaveCount(1);
        result.First().Name.Should().NotBe(recordId);
    }
}