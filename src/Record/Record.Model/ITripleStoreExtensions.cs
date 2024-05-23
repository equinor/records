using VDS.RDF;

namespace Records;

public static class ITripleStoreExtensions
{
    public static IGraph Collapse(this ITripleStore store, string id) => store.Collapse(new Uri(id));
    
    public static IGraph Collapse(this ITripleStore store, Uri id) => store.Collapse(new UriNode(id));
    
    public static IGraph Collapse(this ITripleStore store, IUriNode id) => store.Collapse(id as IRefNode);
    
    public static IGraph Collapse(this ITripleStore store) => store.Collapse(store.Graphs.FirstOrDefault()!.Name ?? new UriNode(new Uri("urn:default")));
    
    public static IGraph Collapse(this ITripleStore store, IRefNode id)
    {
        var graph = new Graph(id);
        foreach (var triple in store.Triples)
            graph.Assert(triple);

        return graph;
    }    

}
