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
    readonly Uri _connectionUri = fusekiContainerManager.address;

    [Fact]
    public async Task CanCreateFusekiRecordBackend()
    {
        ITripleStore store = new TripleStore();
        var graph = await TestData.ValidRecord().TripleStore();
        var writer = new TriGWriter();
        var recordString = VDS.RDF.Writing.StringWriter.Write(graph, writer);
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromTrigAsync(recordString, _connectionUri, () => Task.FromResult(string.Empty));
        Assert.NotNull(backend);
        var record = new Records.Immutable.Record(backend, DescribesConstraintMode.None);
        var result = record.Metadata?.Count;

        result.Should().Be(14);
    }

    [Fact]
    public async Task CanCreateFusekiRecordFromJsonLdRecord()
    {
        var recordString = await TestData.ValidJsonLdRecordString();
        var backend = await Records.Backend.FusekiRecordBackend.CreateFromJsonLdAsync(recordString, _connectionUri, () => Task.FromResult(string.Empty));
        Assert.NotNull(backend);
        var record = new Records.Immutable.Record(backend, DescribesConstraintMode.None);
        var result = record.Metadata!.Count();

        result.Should().Be(14);
    }

}