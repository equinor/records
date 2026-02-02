using Record.Test.TestInfrastructure;

namespace Records.Tests;

public class FusekiRecordBackendTests 
{
    [Fact]
    public void CanCreateFusekiRecordBackend()
    {
        var fusekiManager = new FusekiContainerManager();
        var connectionstring = fusekiManager.CreateClient();
        var backend = new Records.Backend.FusekiRecordBackend(connectionstring);
        Assert.NotNull(backend);
    }
    
}