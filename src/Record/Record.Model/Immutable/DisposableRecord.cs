using Records.Backend;
using VDS.RDF;

namespace Records.Immutable;

public class DisposableRecord : Record, IAsyncDisposable
{
    public DisposableRecord(IRecordBackend backend, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None) : base(backend, describesConstraintMode)
    {
    }

    public DisposableRecord(ITripleStore store, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None) : base(store, describesConstraintMode)
    {
    }

    public DisposableRecord(string rdfString, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None) : base(rdfString, describesConstraintMode)
    {
    }

    public DisposableRecord(IGraph graph, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None) : base(graph, describesConstraintMode)
    {
    }

    public DisposableRecord(string rdfString, IStoreReader reader, DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None) : base(rdfString, reader, describesConstraintMode)
    {
    }

    public async ValueTask DisposeAsync() => await DeleteDatasetAsync();
}