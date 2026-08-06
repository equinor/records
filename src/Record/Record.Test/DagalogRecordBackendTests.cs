using FluentAssertions;
using Record.Test.TestInfrastructure;
using Records.Backend;
using VDS.RDF;
using VDS.RDF.Writing;

namespace Records.Tests;

[Collection("Dagalog Integration Testing Collection")]
public class DagalogRecordBackendTests(DagalogContainerManager dagalogContainerManager)
{
    private readonly HttpClient _httpClient = new() { BaseAddress = dagalogContainerManager.address };
    private UriNode _recordIdUriNode = new UriNode(new Uri("https://ssi.example.com/record/1"));

    [Theory]
    [InlineData(RdfMediaType.JsonLd)]
    [InlineData(RdfMediaType.Trig)]
    [InlineData(RdfMediaType.Quads)]
    public async Task CanCreateDagalogRecordBackend(RdfMediaType rdfMediaType)
    {
        var recordString = await TestData.ValidRecordString(rdfMediaType.GetStoreWriter());
        var backend = await Records.Backend.DagalogRecordBackend.CreateAsync(recordString, rdfMediaType, _httpClient);
        Assert.NotNull(backend);
        var record = await Records.Immutable.Record.CreateAsync(backend, DescribesConstraintMode.None);
        var result = record.Metadata?.Count;
        result.Should().Be(22);
    }

    [Fact]
    public async Task CanCreateDagalogRecordFromJsonLdRecord()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromJsonLdAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var record = await Records.Immutable.Record.CreateAsync(backend, DescribesConstraintMode.None);
        var result = record.Metadata!.Count();
        result.Should().Be(22);
    }

    [Fact]
    public async Task CanBuildRecordWithDagalogBackend()
    {
        Records.Immutable.Record? record = null;

        try
        {
            record = await new RecordBuilder(backendFactory: async () =>
                    (IRecordBuildableBackend)await Backend.DagalogRecordBackend.CreateForBuildAsync(_httpClient))
                .WithId(TestData.CreateRecordId(0))
                .WithScopes(TestData.CreateRecordIri("scope", "0"))
                .WithDescribes(TestData.CreateRecordIri("describes", "0"))
                .WithContent(TestData.CreateRecordTriple("0"))
                .Build();

            record.Id.Should().Be(TestData.CreateRecordId(0));
            record.Scopes.Should().Contain(TestData.CreateRecordIri("scope", "0"));
            record.Describes.Should().Contain(TestData.CreateRecordIri("describes", "0"));
        }
        finally
        {
            if (record is not null)
                await record.DeleteDatasetAsync();
        }
    }

    [Fact]
    public async Task WithAdditionalMetadata_AddsMetadataToExistingDagalogDataset()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var backend = await Backend.DagalogRecordBackend.CreateFromJsonLdAsync(recordString, _httpClient);
        var handleBefore = backend.ExportRecordHandleV1(TimeSpan.FromMinutes(5));

        var additionalMetadata = new Graph();
        additionalMetadata.Assert(new Triple(
            new UriNode(new Uri(handleBefore.RecordId)),
            new UriNode(new Uri("https://rdf.equinor.com/ontology/record/replaces")),
            new UriNode(new Uri(TestData.CreateRecordId("2")))));

        try
        {
            var updatedBackend = await backend.WithAdditionalMetadata(additionalMetadata);
            var handleAfter = backend.ExportRecordHandleV1(TimeSpan.FromMinutes(5));
            var updatedRecord = await Records.Immutable.Record.CreateAsync(updatedBackend, DescribesConstraintMode.None);

            updatedBackend.Should().BeSameAs(backend);
            handleAfter.Dataset.Should().Be(handleBefore.Dataset);
            updatedRecord.Replaces.Should().Contain(TestData.CreateRecordId("2"));
            updatedRecord.Metadata.Should().HaveCount(23);
        }
        finally
        {
            await backend.DeleteDatasetAsync();
        }
    }

    [Fact]
    public async Task ReadLabelTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var labels = await backend.LabelsOfSubject(new UriNode(new Uri("https://ssi.example.com/subject/1")));
        Assert.Single(labels);
    }

    [Fact]
    public async Task GetPredicateObjectTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var triplesWithPredicateAndObject = await backend.TriplesWithPredicateAndObject(
            new UriNode(new Uri("https://ssi.example.com/predicate/1")),
            new UriNode(new Uri("https://ssi.example.com/object/1")));
        Assert.Single(triplesWithPredicateAndObject);
    }

    [Fact]
    public async Task GetSubjectObjectTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var triplesWithSubjectObject = await backend.TriplesWithSubjectObject(
            new UriNode(new Uri("https://ssi.example.com/subject/2")),
            new UriNode(new Uri("https://ssi.example.com/object/2")));
        Assert.Single(triplesWithSubjectObject);
    }

    [Fact]
    public async Task GetSubjectPredicateTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var recordScopes = await backend.TriplesWithSubjectPredicate(
            new UriNode(new Uri("https://ssi.example.com/record/1")),
            new UriNode(new Uri("https://rdf.equinor.com/ontology/record/isInScope")));
        Assert.Equal(5, recordScopes.Count());
    }

    [Fact]
    public async Task SubjectsOfTypes()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var subjectWithType = await backend.SubjectWithType(new UriNode(new Uri("https://rdf.equinor.com/ontology/record/Record")));
        Assert.Single(subjectWithType);
    }

    [Fact]
    public async Task TriplesWithSubject()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);

        var subjectWithType = await backend.TriplesWithSubject(_recordIdUriNode);
        Assert.Equal(15, subjectWithType.Count());
    }

    [Fact]
    public async Task CreateDatasetAsync_IsIdempotent_WhenCalledTwice()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);

        var act = async () => await backend.CreateDatasetAsync();
        await act.Should().NotThrowAsync("a retry of dataset creation should be treated as idempotent");
    }

    [Fact]
    public async Task SparqlInjectionIsBlocked()
    {
        var maliciousInput = "?o \" } } . DELETE WHERE { ?s ?p ?o }";
        INode testNode = new LiteralNode(maliciousInput);

        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);

        var subjectWithType = await backend.TriplesWithObject(testNode);
        Assert.Empty(subjectWithType);
    }

    [Fact]
    public async Task ValidateContentWithShacl_UsesDagalogShaclEndpoint()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);

        var shapeFile = "Data/fuseki-shacl-missing-predicate.ttl";

        try
        {
            var outcome = await backend.ValidateContentWithShacl([shapeFile], TestData.CreateRecordSubject("1"));
            outcome.Conforms.Should().BeFalse();
            outcome.Messages.Should().Contain(message => message.Contains("https://ssi.example.com/predicate/missing"));
        }
        finally
        {
            await backend.DeleteDatasetAsync();
        }
    }

    [Fact]
    public async Task Can_Export_And_Rehydrate_From_RecordHandleV1()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var original = await Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);

        try
        {
            var handle = original.ExportRecordHandleV1(TimeSpan.FromMinutes(5));
            handle.Kind.Should().Be(RecordHandle.RecordHandleV1.KindDagalogDatasetRef);

            var fromHandle = await Backend.DagalogRecordBackend.CreateFromExisting(_httpClient, handle);

            fromHandle.GetRecordId().AbsoluteUri.Should().Be(original.GetRecordId().AbsoluteUri);
            (await fromHandle.Triples()).Count().Should().Be((await original.Triples()).Count());
        }
        finally
        {
            await original.DeleteDatasetAsync();
        }
    }

    [Fact]
    public async Task CreateFromExisting_Rejects_Expired_RecordHandleV1()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        var handle = backend.ExportRecordHandleV1(TimeSpan.FromMinutes(5));

        var expiredHandle = handle with { ExpiresAt = DateTimeOffset.UtcNow.AddSeconds(-1) };

        try
        {
            var act = async () => await Records.Backend.DagalogRecordBackend.CreateFromExisting(_httpClient, expiredHandle);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
        finally
        {
            await backend.DeleteDatasetAsync();
        }
    }

    [Fact]
    public async Task CreateFromExisting_Rejects_FusekiHandle()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.DagalogRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        var dagalogHandle = backend.ExportRecordHandleV1(TimeSpan.FromMinutes(5));

        var fusekiHandle = dagalogHandle with { Kind = RecordHandle.RecordHandleV1.KindFusekiDatasetRef };

        try
        {
            var act = async () => await Records.Backend.DagalogRecordBackend.CreateFromExisting(_httpClient, fusekiHandle);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
        finally
        {
            await backend.DeleteDatasetAsync();
        }
    }
}
