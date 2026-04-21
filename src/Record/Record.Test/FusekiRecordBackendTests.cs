using FluentAssertions;
using Record.Test.TestInfrastructure;
using VDS.RDF;
using VDS.RDF.Writing;

namespace Records.Tests;

[Collection("Integration Testing Collection")]
public class FusekiRecordBackendTests(FusekiContainerManager fusekiContainerManager)
{
    private readonly HttpClient _httpClient = new() { BaseAddress = fusekiContainerManager.address };
    private UriNode _recordIduriNode = new UriNode(new Uri("https://ssi.example.com/record/1"));

    [Theory]
    [InlineData(RdfMediaType.JsonLd)]
    [InlineData(RdfMediaType.Trig)]
    [InlineData(RdfMediaType.Quads)]
    public async Task CanCreateFusekiRecordBackend(RdfMediaType rdfMediaType)
    {
        var recordString = await TestData.ValidRecordString(rdfMediaType.GetStoreWriter());
        var backend = await Records.Backend.FusekiRecordBackend.CreateAsync(recordString, rdfMediaType, _httpClient);
        Assert.NotNull(backend);
        var record = await Records.Immutable.Record.CreateAsync(backend, DescribesConstraintMode.None);
        var result = record.Metadata?.Count;
        result.Should().Be(21);

    }

    [Fact]
    public async Task CanCreateFusekiRecordFromJsonLdRecord()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromJsonLdAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var record = await Records.Immutable.Record.CreateAsync(backend, DescribesConstraintMode.None);
        var result = record.Metadata!.Count();

        result.Should().Be(21);
    }


    [Fact]
    public async Task ReadLabelTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var labels = await backend.LabelsOfSubject(new UriNode(new Uri("https://ssi.example.com/subject/1")));
        Assert.Single(labels);
    }


    [Fact]
    public async Task GetPredicateObjectTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
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
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
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
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
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
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var subjectWithType = await backend.SubjectWithType(new UriNode(new Uri("https://rdf.equinor.com/ontology/record/Record")));
        Assert.Single(subjectWithType);
    }
    [Fact]
    public async Task TriplesWithSubject()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);

        var subjectWithType = await backend.TriplesWithSubject(_recordIduriNode);
        Assert.Equal(14, subjectWithType.Count());
    }

    [Fact]
    public async Task CreateDatasetAsync_IsIdempotent_WhenCalledTwice()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);

        var act = async () => await backend.CreateDatasetAsync();
        await act.Should().NotThrowAsync("a retry of dataset creation should be treated as idempotent");
    }

    [Fact]
    public async Task SparqlInjectionIsBlocked()
    {
        var maliciousInput = "?o \" } } . DELETE WHERE { ?s ?p ?o }";
        INode testNode = new LiteralNode(maliciousInput);

        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);

        var subjectWithType = await backend.TriplesWithObject(testNode);
        Assert.Empty(subjectWithType);
    }
}