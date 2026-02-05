
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Records.Backend;

namespace Record.Test.TestInfrastructure;

public class FusekiContainerManager : IAsyncLifetime
{
    private const string _imageName = "bravo-jena-test";
    private const int _fusekiPort = 3030;
    private IContainer? _fusekiContainer;

    public Uri address =>
           new Uri($"http://{_fusekiContainer!.Hostname}:{_fusekiContainer!.GetMappedPublicPort(_fusekiPort)}");

    public async Task InitializeAsync()
    {
        var commonDirectoryPath = CommonDirectoryPath.GetBinDirectory();
        var futureImage = new ImageFromDockerfileBuilder()
            .WithName(_imageName)
            .WithDockerfileDirectory(commonDirectoryPath, "docker-fuseki")
            .WithDockerfile("Dockerfile")
            .WithCleanUp(true)
            .Build();

        await futureImage.CreateAsync().ConfigureAwait(false);

        _fusekiContainer = new ContainerBuilder(_imageName)
            .WithPortBinding(_fusekiPort, true)
            .WithEnvironment("ADMIN_PASSWORD", "admin")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(_fusekiPort))
            .WithCleanUp(true)
            .Build();

        await _fusekiContainer
                .StartAsync()
                .ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _fusekiContainer!
                .StopAsync()
                .ConfigureAwait(false);
    }


}
