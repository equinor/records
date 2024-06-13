using VDS.RDF;

namespace Records.Utils;

public static class CanonicalisationExtensions
{
    public static ITripleStore Canonicalise(this ITripleStore store) => new RdfCanonicalizer().Canonicalize(store).OutputDataset;

    public static IEnumerable<Triple> Canonicalise(this IEnumerable<Triple> triples)
    {
        var store = new TripleStore();
        var graph = new Graph();
        foreach (var triple in triples) graph.Assert(triple);
        store.Add(graph);

        return store.Canonicalise().Triples;
    }

    public static IGraph Canonicalise(this IGraph graph)
    {
        var store = new TripleStore();
        store.Add(graph);
        return store.Canonicalise().Graphs.Single();
    }
}
