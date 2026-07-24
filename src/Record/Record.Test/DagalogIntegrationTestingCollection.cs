using Record.Test.TestInfrastructure;

namespace Records.Tests;

[CollectionDefinition("Dagalog Integration Testing Collection", DisableParallelization = false)]
public class DagalogIntegrationTestingCollection : ICollectionFixture<DagalogContainerManager>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}
