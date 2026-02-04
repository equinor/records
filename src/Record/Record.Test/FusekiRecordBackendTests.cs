using Record.Test.TestInfrastructure;
using VDS.RDF;
using VDS.RDF.Writing;
using StringWriter = System.IO.StringWriter;

namespace Records.Tests;

public class FusekiRecordBackendTests(FusekiContainerManager fusekiContainerManager) : IClassFixture<FusekiContainerManager>, IAsyncLifetime
{
    [Fact]
    public async Task CanCreateFusekiRecordBackend()
    {
        var connectionstring = fusekiContainerManager.address;
        var backend = Records.Backend.FusekiRecordBackend.CreateAsync(connectionstring, () => Task.FromResult(string.Empty) );
        Assert.NotNull(backend);
        ITripleStore store = new TripleStore();
        var graph = await TestData.ValidRecord().TripleStore();
        var writer = new TriGWriter();
        var recordString = VDS.RDF.Writing.StringWriter.Write(graph, writer);
        await backend.UploadRdfData("http://example.com/record/1", recordString);
    var record = new Records.Immutable.Record(backend, DescribesConstraintMode.None);
    }

    public Task InitializeAsync() => fusekiContainerManager.InitializeAsync();

    public Task DisposeAsync() => fusekiContainerManager.DisposeAsync();
    
}