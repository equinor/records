using Records.Exceptions;
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

    public static IEnumerable<Immutable.Record> FindRecords(this TripleStore store)
    {
        var typeNode = Namespaces.Rdf.UriNodes.Type;
        var recordTypeNode = Namespaces.Record.UriNodes.RecordType;
        var hasContentNode = Namespaces.Record.UriNodes.HasContent;

        var nonEmptyGraphs = store.Graphs.Where(graph => graph.Triples.Any() && graph.Name != null);

        var blankGraphs = nonEmptyGraphs.Where(graph => graph.Name is BlankNode);

        if (blankGraphs.Any())
            throw new RecordException($"You cannot use blank graphs in a collection of records.");

        var recordGraphs = nonEmptyGraphs.Where(graph => graph.ContainsTriple(new(graph.Name, typeNode, recordTypeNode)));
        var contentGraphs = nonEmptyGraphs.Where(graph => !recordGraphs.Contains(graph));


        var records = new List<Immutable.Record>();

        foreach (var contentGraph in contentGraphs)
        {
            var tripleReferences = store.GetTriplesWithPredicateObject(hasContentNode, contentGraph.Name);

            if (tripleReferences.Count() != 1)
                throw new RecordException("A content graph must be referenced by exactly one record.");
        }

        foreach (var graph in recordGraphs)
        {
            var contentGraphIds = graph
                .GetTriplesWithPredicate(hasContentNode)
                .Select(t => t.Object);

            var localContentGraphs = store.Graphs
                .Where(graph => contentGraphIds.Contains(graph.Name));

            var recordStore = new TripleStore();
            recordStore.Add(graph);
            foreach (var contentGraph in localContentGraphs)
                recordStore.Add(contentGraph);

            records.Add(new Immutable.Record(recordStore));
        }

        return records;
    }
}
