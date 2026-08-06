using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace Record.Test.TestInfrastructure;

public class DagalogContainerManager : IAsyncLifetime
{
    private const string _imageName = "ghcr.io/daghovland/dagalog:0.4.1";
    private const int _dagalogPort = 3030;
    private IContainer? _dagalogContainer;

    public Uri address =>
        new Uri($"http://{_dagalogContainer!.Hostname}:{_dagalogContainer!.GetMappedPublicPort(_dagalogPort)}");

    public async Task InitializeAsync()
    {
        _dagalogContainer = new ContainerBuilder()
            .WithImage(_imageName)
            .WithPortBinding(_dagalogPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilInternalTcpPortIsAvailable(_dagalogPort))
            .WithCleanUp(true)
            .Build();

        await _dagalogContainer
            .StartAsync()
            .ConfigureAwait(false);
    }

    public async Task DisposeAsync()
    {
        await _dagalogContainer!
            .StopAsync()
            .ConfigureAwait(false);
    }
}
