using System.Net.Http.Headers;
using FluentAssertions;
using Record.Test.TestInfrastructure;
using VDS.RDF;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;

namespace Records.Tests;

[Collection("Integration Testing Collection")]
public class FusekiRecordBackendTests(FusekiContainerManager fusekiContainerManager)
{
    private readonly HttpClient  _httpClient = new(){BaseAddress = fusekiContainerManager.address};
    private UriNode _recordIduriNode = new UriNode(new Uri("https://ssi.example.com/record/1"));

    [Theory]
    [InlineData(RdfMediaType.JsonLd)]
    [InlineData(RdfMediaType.Trig)]
    [InlineData(RdfMediaType.Quads)]
    public async Task CanCreateFusekiRecordBackend(RdfMediaType rdfMediaType)
    {
        var recordString = await TestData.ValidRecordString(rdfMediaType.GetStoreWriter());
        var backend = await Records.Backend.FusekiRecordBackend.CreateAsync( recordString, rdfMediaType, _httpClient);
        Assert.NotNull(backend);
        var record = new Records.Immutable.Record(backend, DescribesConstraintMode.None);
        var result = record.Metadata?.Count;
        result.Should().Be(14);
        
    }

    [Fact]
    public async Task CanCreateFusekiRecordFromJsonLdRecord()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromJsonLdAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var record = new Records.Immutable.Record(backend, DescribesConstraintMode.None);
        var result = record.Metadata!.Count();

        result.Should().Be(14);
    }

    
    [Fact]
    public async Task ReadLabelTriples()
    {
        var recordString = await TestData.ValidRecordString<TriGWriter>();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _httpClient);
        Assert.NotNull(backend);
        var labels = await backend.LabelsOfSubject(new UriNode(new Uri("https://example.com/record/1")));
        Assert.Empty(labels);
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
}