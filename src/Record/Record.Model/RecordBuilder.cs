using System.Reflection;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using VDS.RDF.Shacl.Validation;
using Triple = VDS.RDF.Triple;
using Record = Records.Immutable.Record;
using static Records.ProvenanceBuilder;
using Records.Utils;
using System.Security.Cryptography;
using System.Text;
using System.Collections;
using ICSharpCode.SharpZipLib.Checksum;

namespace Records;

public record RecordBuilder
{
    private Storage _storage;
    private ProvenanceBuilder _metadataProvenance;
    private ProvenanceBuilder _contentProvenance;
    private ShapesGraph _processor;

    public RecordBuilder()
    {
        _storage = new Storage();
        _metadataProvenance =
            WithAdditionalTool(CreateRecordVersionUri())
            (WithAdditionalComments("This is the process that generated the record metadata/provenance")
            (new ProvenanceBuilder())
            );

        _contentProvenance =
            WithAdditionalComments(
                "This is the process that generated the record content. In later versions of the record library this will be on a separate content graph")
        (new ProvenanceBuilder());
        var shapes = new Graph();
        var outputFolderPath = Assembly.GetExecutingAssembly()
                                   .GetManifestResourceStream("Records.Schema.record-single-syntax.shacl.ttl") ??
                               throw new Exception("Could not get assembly path.");
        var shapeString = new StreamReader(outputFolderPath).ReadToEnd();
        shapes.LoadFromString(shapeString);
        _processor = new ShapesGraph(shapes);
    }

    #region With-Methods

    #region Metadata-Methods

    #region With-Metadata-Methods

    public RecordBuilder WithScopes(params string[] scopes) =>
        this with
        {
            _storage = _storage with
            {
                Scopes = scopes.ToList()
            }
        };

    public RecordBuilder WithScopes(IEnumerable<string> scopes)
        => WithScopes(scopes.ToArray());

    public RecordBuilder WithScopes(params Uri[] scopes) => WithScopes(scopes.Select(s => s.ToString()));

    public RecordBuilder WithDescribes(params string[] describes) =>
        this with
        {
            _storage = _storage with
            {
                Describes = describes.ToList()
            }
        };

    public RecordBuilder WithDescribes(IEnumerable<string> describes)
        => WithDescribes(describes.ToArray());

    public RecordBuilder WithDescribes(params Uri[] describes) => WithDescribes(describes.Select(d => d.ToString()));

    public RecordBuilder WithReplaces(params string[] replaces) =>
        this with
        {
            _storage = _storage with
            {
                Replaces = replaces.ToList()
            }
        };


    public RecordBuilder WithReplaces(IEnumerable<string> replaces) => WithReplaces(replaces.ToArray());

    public RecordBuilder WithReplaces(params Uri[] replaces) => WithReplaces(replaces.Select(r => r.ToString()));

    public RecordBuilder WithIsSubRecordOf(string isSubRecordOf) =>
        this with
        {
            _storage = _storage with
            {
                IsSubRecordOf = isSubRecordOf
            }
        };

    public RecordBuilder WithId(Uri id) =>
        this with
        {
            _storage = _storage with
            {
                Id = id
            }
        };

    public RecordBuilder WithId(string id) => WithId(new Uri(id));

    #endregion

    #region ProvenanceBuilderWrappers

    public RecordBuilder WithAdditionalContentProvenance(params Func<ProvenanceBuilder, ProvenanceBuilder>[] provenanceBuilders) =>
        this with
        {
            _contentProvenance = provenanceBuilders.Aggregate(
                _contentProvenance,
                (prov, provenanceBuilder) => provenanceBuilder(prov))
        };


    public RecordBuilder WithAdditionalMetadataProvenance(params Func<ProvenanceBuilder, ProvenanceBuilder>[] provenanceBuilders) =>
        this with
        {
            _metadataProvenance = provenanceBuilders.Aggregate(
                _metadataProvenance,
                (prov, provenanceBuilder) => provenanceBuilder(prov))
        };

    #endregion

    #region With-Additional-Metadata-Methods

    public RecordBuilder WithAdditionalScopes(params string[] scopes) =>
        this with
        {
            _storage = _storage with
            {
                Scopes = _storage.Scopes.Concat(scopes).ToList()
            }
        };
    public RecordBuilder WithAdditionalScopes(IEnumerable<string> scopes)
        => WithAdditionalScopes(scopes.ToArray());
    public RecordBuilder WithAdditionalScopes(params Uri[] scopes) => WithAdditionalScopes(scopes.Select(s => s.ToString()));

    public RecordBuilder WithAdditionalDescribes(params string[] describes) =>
        this with
        {
            _storage = _storage with
            {
                Describes = _storage.Describes.Concat(describes).ToList()
            }
        };

    public RecordBuilder WithAdditionalDescribes(IEnumerable<string> describes)
        => WithAdditionalDescribes(describes.ToArray());

    public RecordBuilder WithAdditionalDescribes(params Uri[] describes) => WithAdditionalDescribes(describes.Select(d => d.ToString()));

    public RecordBuilder WithAdditionalReplaces(params string[] replaces) =>
        this with
        {
            _storage = _storage with
            {
                Replaces = _storage.Replaces.Concat(replaces).ToList()
            }
        };

    public RecordBuilder WithAdditionalReplaces(IEnumerable<string> replaces) =>
        WithAdditionalReplaces(replaces.ToArray());

    public RecordBuilder WithAdditionalReplaces(params Uri[] replaces) => WithAdditionalReplaces(replaces.Select(r => r.ToString()));

    public RecordBuilder WithAdditionalMetadata(params IGraph[] graphs) =>
        this with
        {
            _storage = _storage with
            {
                MetadataGraphs = _storage.MetadataGraphs.Concat(graphs).ToList(),
                MetadataRdfStrings = _storage.MetadataRdfStrings.ToList(),
                MetadataTriples = _storage.MetadataTriples.ToList()
            }
        };

    public RecordBuilder WithAdditionalMetadata(IEnumerable<IGraph> graphs) => WithAdditionalMetadata(graphs.ToArray());

    public RecordBuilder WithAdditionalMetadata(params Triple[] triples) =>
        this with
        {
            _storage = _storage with
            {
                MetadataTriples = _storage.Triples.Concat(triples).ToList(),
                MetadataRdfStrings = _storage.RdfStrings.ToList(),
                MetadataGraphs = _storage.ContentGraphs.ToList()
            }
        };

    public RecordBuilder WithAdditionalMetadata(IEnumerable<Triple> triples) => WithAdditionalMetadata(triples.ToArray());

    public RecordBuilder WithAdditionalMetadata(params string[] rdfStrings) =>
        this with
        {
            _storage = _storage with
            {
                MetadataRdfStrings = _storage.RdfStrings.Concat(rdfStrings).ToList(),
                MetadataTriples = _storage.Triples.ToList(),
                MetadataGraphs = _storage.ContentGraphs.ToList()
            }
        };

    public RecordBuilder WithAdditionalMetadata(IEnumerable<string> rdfStrings) => WithAdditionalMetadata(rdfStrings.ToArray());


    #endregion
    #endregion

    #region Content-Methods
    #region With-Content
    public RecordBuilder WithContent(params IGraph[] graphs) =>
    this with
    {
        _storage = _storage with
        {
            ContentGraphs = graphs.ToList(),
            RdfStrings = new(),
            Triples = new(),
            Quads = new()
        }

    };
    public RecordBuilder WithContent(IEnumerable<IGraph> graphs) => WithContent(graphs.ToArray());

    public RecordBuilder WithContent(params Quad[] quads) =>
        this with
        {
            _storage = _storage with
            {
                Quads = quads.ToList(),
                RdfStrings = new(),
                Triples = new(),
                ContentGraphs = new()
            }
        };

    public RecordBuilder WithContent(IEnumerable<Quad> quads) => WithContent(quads.ToArray());

    public RecordBuilder WithContent(params Triple[] triples) =>
        this with
        {
            _storage = _storage with
            {
                Triples = triples.ToList(),
                Quads = new(),
                RdfStrings = new(),
                ContentGraphs = new()
            }
        };
    public RecordBuilder WithContent(IEnumerable<Triple> triples) => WithContent(triples.ToArray());

    public RecordBuilder WithContent(params string[] rdfStrings) =>
        this with
        {
            _storage = _storage with
            {
                RdfStrings = rdfStrings.ToList(),
                Triples = new(),
                Quads = new(),
                ContentGraphs = new()
            }
        };
    public RecordBuilder WithContent(IEnumerable<string> rdfStrings) => WithContent(rdfStrings.ToArray());
    #endregion

    #region With-Additional-Content
    public RecordBuilder WithAdditionalContent(params IGraph[] graphs) =>
    this with
    {
        _storage = _storage with
        {
            ContentGraphs = _storage.ContentGraphs.Concat(graphs).ToList(),
            Quads = _storage.Quads.ToList(),
            Triples = _storage.Triples.ToList(),
            RdfStrings = _storage.RdfStrings.ToList()
        }
    };

    public RecordBuilder WithAdditionalContent(IEnumerable<IGraph> graphs) => WithAdditionalContent(graphs.ToArray());

    public RecordBuilder WithAdditionalContent(params Triple[] triples) =>
        this with
        {
            _storage = _storage with
            {
                Triples = _storage.Triples.Concat(triples).ToList(),
                Quads = _storage.Quads.ToList(),
                RdfStrings = _storage.RdfStrings.ToList(),
                ContentGraphs = _storage.ContentGraphs.ToList()
            }
        };
    public RecordBuilder WithAdditionalContent(IEnumerable<Triple> triples) => WithAdditionalContent(triples.ToArray());

    public RecordBuilder WithAdditionalContent(params Quad[] quads) =>
        this with
        {
            _storage = _storage with
            {
                Quads = _storage.Quads.Concat(quads).ToList(),
                Triples = _storage.Triples.ToList(),
                RdfStrings = _storage.RdfStrings.ToList(),
                ContentGraphs = _storage.ContentGraphs.ToList()
            }
        };

    public RecordBuilder WithAdditionalContent(IEnumerable<Quad> quads) => WithAdditionalContent(quads.ToArray());

    public RecordBuilder WithAdditionalContent(params string[] rdfStrings) =>
        this with
        {
            _storage = _storage with
            {
                RdfStrings = _storage.RdfStrings.Concat(rdfStrings).ToList(),
                Quads = _storage.Quads.ToList(),
                Triples = _storage.Triples.ToList(),
                ContentGraphs = _storage.ContentGraphs.ToList()
            }
        };
    public RecordBuilder WithAdditionalContent(IEnumerable<string> rdfStrings) => WithAdditionalContent(rdfStrings.ToArray());
    #endregion
    #endregion
    #endregion

    public Record Build()
    {
        if (_storage.Id == null) throw new RecordException("Record needs ID.");

        var metadataGraph = CreateMetadataGraph();

        var contentGraphId = metadataGraph.CreateBlankNode();

        var contentGraph = CreateContentGraph(contentGraphId, metadataGraph);
        var checkSumTriples = CreateChecksumTriples(_storage.ContentGraphs.Append(contentGraph));

        metadataGraph.Assert(new Triple(new UriNode(_storage.Id), new UriNode(new Uri(Namespaces.Record.HasContent)), contentGraphId));

        var ts = CreateTripleStore(metadataGraph, contentGraph);

        return new Record(ts);
    }

    private static IEnumerable<Triple> CreateChecksumTriples(IEnumerable<IGraph> contentGraphs)
    {
        IEnumerable<(string graphId, string value)> checkSums = contentGraphs
            .Select(CanonicalisationExtensions.Canonicalise)
            .Select(g =>
            (
                graphId: g.Name.ToString(),
                value: Encoding.UTF8.GetString(MD5.HashData(Encoding.ASCII.GetBytes(g.ToSafeString())))
            ));

        return checkSums.Select(cs =>
            {
                var graph = new Graph();
                var checkSumNode = graph.CreateBlankNode();

                graph.Assert(new Triple(graph.CreateUriNode(cs.graphId), graph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksum)), checkSumNode));
                graph.Assert(new Triple(checkSumNode, graph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumAlgorithm)), graph.CreateUriNode(new Uri($"{Namespaces.FileContent.Spdx}checksumAlgorithm_md5"))));
                graph.Assert(new Triple(checkSumNode, graph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumValue)), graph.CreateLiteralNode(cs.value, new Uri(Namespaces.DataType.HexBinary))));

                return graph.Triples;
            }).SelectMany(g => g);
    }

    #region Private-Builder-Helper-Methods
    private Graph CreateMetadataGraph()
    {
        var metadataGraph = new Graph(_storage.Id);
        metadataGraph.BaseUri = _storage.Id;

        var recordPredicates = GetRecordPredicates();

        var metadataTripleString = GetMetadataTripleString(recordPredicates);
        metadataGraph.LoadFromString(metadataTripleString);

        CheckMetadataGraph(recordPredicates);

        _storage.MetadataGraphs.ForEach(metadataGraph.Merge);
        metadataGraph.Assert(_metadataProvenance.Build(metadataGraph, metadataGraph.Name));
        metadataGraph.Assert(_contentProvenance.Build(metadataGraph, metadataGraph.Name));

        return metadataGraph;
    }

    private Graph CreateContentGraph(IBlankNode contentGraphId, Graph metadataGraph)
    {
        var contentGraph = new Graph(contentGraphId);

        var tripleString = CreateContentTripleString();
        contentGraph.LoadFromString(tripleString);

        CheckContentGraph();

        var report = _processor.Validate(metadataGraph);
        if (!report.Conforms) throw ShaclException(report);

        return contentGraph;
    }

    private TripleStore CreateTripleStore(Graph metadataGraph, Graph contentGraph)
    {
        var ts = new TripleStore();

        foreach (var graph in _storage.ContentGraphs)
        {
            ts.Add(graph);
            metadataGraph.Assert(new Triple(new UriNode(_storage.Id), new UriNode(new Uri(Namespaces.Record.HasContent)), graph.Name));
        }

        ts.Add(metadataGraph);
        ts.Add(contentGraph);

        return ts;
    }

    private void CheckMetadataGraph(IEnumerable<string> recordPredicates)
    {
        foreach (var graph in _storage.MetadataGraphs.Select(g => g.Triples))
            if (graph.Any(t => !t.Subject.ToString().Equals(_storage.Id.ToString()) && recordPredicates.Contains(t.Predicate.ToString())))
                throw new RecordException("For all triples where the predicate is in the record ontology, the subject must be the record itself.");
    }

    private string GetMetadataTripleString(IEnumerable<string> recordPredicates)
    {
        var metadataQuads = CreateMetadataQuads();
        var additionalMetadataQuads = CreateAdditionalMetadataQuads(recordPredicates);
        metadataQuads.AddRange(additionalMetadataQuads);
        var metadataTripleString = string.Join("\n", metadataQuads.Select(q => q.ToTripleString()));
        return metadataTripleString;
    }

    private List<SafeQuad> CreateAdditionalMetadataQuads(IEnumerable<string> recordPredicates)
    {
        var additionalMetadataQuads = new List<SafeQuad>();
        additionalMetadataQuads.AddRange(_storage.MetadataTriples.Select(CreateQuadFromTriple));
        additionalMetadataQuads.AddRange(_storage.MetadataRdfStrings.SelectMany(SafeQuadListFromRdfString));

        if (additionalMetadataQuads.Any(q => !q.Subject.Equals($"<{_storage.Id.ToString()}>") && recordPredicates.Contains(q.Predicate)))
            throw new RecordException("For all triples where the predicate is in the record ontology, the subject must be the record itself.");

        return additionalMetadataQuads;
    }

    private List<SafeQuad> CreateMetadataQuads()
    {
        var metadataQuads = new List<SafeQuad>();
        var typeQuad = CreateQuadWithPredicateAndObject(Namespaces.Rdf.Type, Namespaces.Record.RecordType);
        metadataQuads.Add(typeQuad);

        if (_storage.IsSubRecordOf != null)
            metadataQuads.Add(CreateIsSubRecordOfQuad(_storage.IsSubRecordOf));

        metadataQuads.AddRange(_storage.Replaces.Select(CreateReplacesQuad));
        metadataQuads.AddRange(_storage.Scopes.Select(CreateScopeQuad));
        metadataQuads.AddRange(_storage.Describes.Select(CreateDescribesQuad));

        return metadataQuads;
    }

    private static IEnumerable<string> GetRecordPredicates()
    {
        return typeof(Namespaces.Record).GetFields()
            .Where(f => f.IsLiteral && !f.IsInitOnly)
            .Select(f => f.GetValue(null)!.ToString())
            .Select(p => $"<{p}>");
    }

    private void CheckContentGraph()
    {
        foreach (var graph in _storage.ContentGraphs.Select(g => g.Triples))
            if (graph.Any(t => t.Subject.ToString().Equals(_storage.Id.ToString())))
                throw new RecordException("Content may not make metadata statements.");
    }

    private string CreateContentTripleString()
    {
        var contentQuads = new List<SafeQuad>();
        contentQuads.AddRange(_storage.Quads.Select(quad =>
        {
            return quad switch
            {
                SafeQuad safeQuad => safeQuad,
                UnsafeQuad unsafeQuad => unsafeQuad.MakeSafe(),
                _ => throw new QuadException($"You cannot use {nameof(Quad)} directly.")
            };
        }));

        contentQuads.AddRange(_storage.Triples.Select(CreateQuadFromTriple));
        contentQuads.AddRange(_storage.RdfStrings.SelectMany(SafeQuadListFromRdfString));

        if (contentQuads.Any(q => q.Subject.Equals($"<{_storage.Id.ToString()}>")))
            throw new RecordException("Content may not make metadata statements.");

        var tripleString = string.Join("\n", contentQuads.Select(q => q.ToTripleString()));
        return tripleString;
    }
    #endregion

    #region Private-Helper-Methods
    private Exception ShaclException(Report report)
    {
        var validationStore = new TripleStore();
        validationStore.Add(report.Graph);
        var messageNode = report.Graph.GetUriNode(new Uri(Namespaces.Shacl.ResultMessage));

        var errorMessages = validationStore
            .GetTriplesWithPredicate(messageNode)
            .Select(t => t.Object.ToSafeString().Split("^^http").First())
            .ToList();

        return new RecordException(string.Join('\n', errorMessages));
    }

    private List<SafeQuad> SafeQuadListFromRdfString(string rdfString)
    {
        var tempStore = new TripleStore();
        try { tempStore.LoadFromString(rdfString); }
        catch { tempStore.LoadFromString(rdfString, new JsonLdParser()); }

        if (tempStore.Graphs.Count != 1) throw new RecordException("Input can only contain one graph.");

        var tempStoreGraph = tempStore.Graphs.FirstOrDefault();
        if (tempStoreGraph == null) throw new UnloadedRecordException();

        return tempStore.Graphs.First().Triples.Select(triple => Quad.CreateSafe(triple, _storage.Id.ToString())).ToList();
    }

    private string CreateRecordVersionUri() =>
        $"https://www.nuget.org/packages/Record/{GetType().Assembly.GetName().Version}";


    private SafeQuad CreateQuadWithPredicateAndObject(string predicate, string @object)
    {
        if (_storage.Id == null) throw new RecordException("Record ID must be added first.");
        return Quad.CreateSafe(_storage.Id.ToString(), predicate, @object, _storage.Id.ToString());
    }

    private SafeQuad CreateQuadFromTriple(Triple triple)
    {
        if (_storage.Id == null) throw new RecordException("Record ID must be added first.");
        return Quad.CreateSafe(triple, _storage.Id.ToString());
    }

    private SafeQuad CreateIsSubRecordOfQuad(string subRecordOf) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.IsSubRecordOf, subRecordOf);

    private SafeQuad CreateScopeQuad(string scope) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.IsInScope, scope);

    private SafeQuad CreateDescribesQuad(string describes) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.Describes, describes);

    private SafeQuad CreateReplacesQuad(string replaces) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.Replaces, replaces);


    #endregion

    private record Storage
    {
        internal Uri? Id;
        internal string? IsSubRecordOf;
        internal List<string> Replaces = new();
        internal List<string> Scopes = new();
        internal List<string> Describes = new();
        internal List<string> RdfStrings = new();
        internal List<Quad> Quads = new();
        internal List<Triple> Triples = new();
        internal List<IGraph> ContentGraphs = new();
        internal List<Triple> MetadataTriples = new();
        internal List<string> MetadataRdfStrings = new();
        internal List<IGraph> MetadataGraphs = new();
    }
}