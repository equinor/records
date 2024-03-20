using VDS.RDF;
using VDS.RDF.Query.Algebra;
using Graph = VDS.RDF.Query.Algebra.Graph;

namespace Records;

public record ProvenanceBuilder
{
    private Storage _storage = new();

    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithUsing(IEnumerable<string> used) => WithUsing(used.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithUsing(params string[] used) =>
        (builder) =>
            builder with
            {
                _storage = builder._storage with
                {
                    Using = builder._storage.Using.Concat(used).ToList()
                }
            };
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithLocation(IEnumerable<string> locations) => WithLocation(locations.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithLocation(params string[] locations) =>
        (builder) =>
            builder with
            {
                _storage = builder._storage with
                {
                    AtLocation = builder._storage.AtLocation.Concat(locations).ToList()
                }
            };

    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithTool(IEnumerable<string> tools) => WithTool(tools.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithTool(params string[] tools) =>
        (builder) =>
        builder with
        {
            _storage = builder._storage with
            {
                With = builder._storage.With.Concat(tools).ToList()
            }
        };
    public IEnumerable<Triple> Build(IGraph graph)
    {

        IRefNode activity = graph.CreateBlankNode();

        var provenanceTriples = new List<Triple>();
        var recordNode = graph.Name;
        provenanceTriples.Add(
            new Triple(
                recordNode,
                graph.CreateUriNode(new Uri(Namespaces.Prov.WasGeneratedBy)),
                activity)
        );
        foreach (var (objectList, property) in new (List<string>, Uri)[]
                 {
                     (_storage.Using, new Uri(Namespaces.Prov.Used)),
                     (_storage.With, new Uri(Namespaces.Prov.WasAssociatedWith)),
                     (_storage.AtLocation, new Uri(Namespaces.Prov.AtLocation))
                 })
        {
            provenanceTriples.AddRange(
                CreateProvenanceTriples(
                    graph,
                    activity,
                    objectList,
                    property)
            );
        }
        return provenanceTriples;
    }

    private IEnumerable<Triple> CreateProvenanceTriples(INodeFactory graph, IRefNode activity, List<string> provenanceObjects,
        Uri property) =>
        provenanceObjects.Select(used =>
            new Triple(
                activity,
                graph.CreateUriNode(property),
                graph.CreateUriNode(new Uri(used))
            ));

    internal record Storage
    {
        internal List<string> Using = new();
        internal List<string> With = new();
        internal List<string> AtLocation = new();
    };
}

