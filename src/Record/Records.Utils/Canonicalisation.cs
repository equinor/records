using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using VDS.RDF;

namespace Records.Utils;

public static class CanonicalisationExtensions
{
    public static ITripleStore Canonicalise(this ITripleStore store) => new RdfCanonicalizer().Canonicalize(store).OutputDataset;

    public static IEnumerable<Triple> Canonicalise(this IEnumerable<Triple> triples) => PutTriplesInStore(triples).Canonicalise().Triples;

    public static IGraph Canonicalise(this IGraph graph) => PutGraphInStore(graph).Canonicalise().Graphs.Single();

    public static string HashGraph(IGraph graph)
    {
        var canonicalisedGraph = Canonicalise(graph);
        var graphAsString = JsonConvert.SerializeObject(canonicalisedGraph);
        var hashBytes = MD5.HashData(Encoding.ASCII.GetBytes(graphAsString));
        var sb = new StringBuilder();
        for (int i = 0; i < hashBytes.Length; i++)
        {
            sb.Append(hashBytes[i].ToString("X2"));
        }
        return sb.ToString();
    }

    private static TripleStore PutTriplesInStore(IEnumerable<Triple> triples)
    {
        var graph = new Graph();
        foreach (var triple in triples) graph.Assert(triple);
        return PutGraphInStore(graph);
    }

    private static TripleStore PutGraphInStore(IGraph graph)
    {
        var store = new TripleStore();
        store.Add(graph);
        return store;
    }
}
