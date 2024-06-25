using VDS.RDF;

namespace Records;

public record ProvenanceBuilder
{
    private Storage _storage = new();

    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalUsing(IEnumerable<string> used) => WithAdditionalUsing(used.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalUsing(params string[] used) =>
        (builder) =>
            builder with
            {
                _storage = builder._storage with
                {
                    Using = builder._storage.Using.Concat(used).ToList()
                }
            };
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalLocation(IEnumerable<string> locations) => WithAdditionalLocation(locations.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalLocation(params string[] locations) =>
        (builder) =>
            builder with
            {
                _storage = builder._storage with
                {
                    AtLocation = builder._storage.AtLocation.Concat(locations).ToList()
                }
            };

    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalComments(IEnumerable<string> comments) => WithAdditionalComments(comments.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalComments(params string[] comments) =>
        (builder) =>
            builder with
            {
                _storage = builder._storage with
                {
                    Comments = builder._storage.Comments.Concat(comments).ToList()
                }
            };

    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalTool(IEnumerable<string> tools) => WithAdditionalTool(tools.ToArray());
    public static Func<ProvenanceBuilder, ProvenanceBuilder> WithAdditionalTool(params string[] tools) =>
        (builder) =>
        builder with
        {
            _storage = builder._storage with
            {
                With = builder._storage.With.Concat(tools).ToList()
            }
        };

    public IEnumerable<Triple> Build(INodeFactory graph, IRefNode rootObject)
    {
        IRefNode activity = graph.CreateBlankNode();

        var provenanceTriples = new List<Triple>();
        provenanceTriples.Add(
            new Triple(
                rootObject,
                Namespaces.Prov.UriNodes.WasGeneratedBy,
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
                    activity,
                    objectList,
                    property)
            );
        }

        provenanceTriples.AddRange(_storage.Comments.Select(comment =>
            new Triple(
                activity,
                Namespaces.Rdfs.UriNodes.Comment,
                new LiteralNode(comment))
        ));
        return provenanceTriples;
    }

    private IEnumerable<Triple> CreateProvenanceTriples(IRefNode activity, List<string> provenanceObjects,
        Uri property) =>
        provenanceObjects.Select(used =>
            new Triple(
                activity,
                new UriNode(property),
                new UriNode(new Uri(used))
            ));

    internal record Storage
    {
        internal List<string> Using = new();
        internal List<string> With = new();
        internal List<string> AtLocation = new();
        internal List<string> Comments = new();
    };
}

