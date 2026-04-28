using System.Reflection;
using Records.Backend;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using Triple = VDS.RDF.Triple;
using Record = Records.Immutable.Record;
using static Records.ProvenanceBuilder;
using Records.Utils;

namespace Records;

public record RecordBuilder
{
    private Storage _storage;
    private ProvenanceBuilder _metadataProvenance;
    private ProvenanceBuilder _contentProvenance;
    private readonly Func<Task<IRecordBuildableBackend>> _backendFactory;
    private ShaclValidationRequest? _shaclValidationRequest;

    public ShaclValidationOutcome? LastShaclValidationOutcome { get; private set; }

    /// <summary>
    /// Creates a new RecordBuilder.
    /// </summary>
    /// <param name="canon">Canonicalisation mode for content graph checksums.</param>
    /// <param name="describesConstraintMode">Constraint mode for the describes predicate.</param>
    /// <param name="backendFactory">
    /// Async factory that produces a fresh empty <see cref="IRecordBuildableBackend"/> for
    /// each <see cref="Build"/> call. Defaults to <see cref="DotNetRdfRecordBackend"/>.
    /// For Fuseki use <c>() => FusekiRecordBackend.CreateForBuildAsync(httpClient)</c>.
    /// </param>
    public RecordBuilder(
        RecordCanonicalisation canon = RecordCanonicalisation.None,
        DescribesConstraintMode describesConstraintMode = DescribesConstraintMode.None,
        Func<Task<IRecordBuildableBackend>>? backendFactory = null)
    {
        _backendFactory = backendFactory
            ?? (() => Task.FromResult<IRecordBuildableBackend>(new DotNetRdfRecordBackend()));

        _storage = new Storage
        {
            DescribesConstraintMode = describesConstraintMode,
            Canon = canon,
        };

        _metadataProvenance =
            WithAdditionalTool(CreateRecordVersionUri())
            (WithAdditionalComments("This is the process that generated the record metadata/provenance")
            (new ProvenanceBuilder())
            );

        _contentProvenance =
            WithAdditionalComments(
                "This is the process that generated the record content.")
        (new ProvenanceBuilder());
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

    public RecordBuilder WithRelated(params string[] related) =>
        this with
        {
            _storage = _storage with
            {
                Related = [.. related]
            }
        };

    public RecordBuilder WithRelated(IEnumerable<string> related)
        => WithRelated([.. related]);

    public RecordBuilder WithRelated(params Uri[] related) => WithRelated(related.Select(r => r.ToString()));

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
                MetadataTriples = _storage.MetadataTriples.Concat(triples).ToList(),
                MetadataRdfStrings = _storage.MetadataRdfStrings.ToList(),
                MetadataGraphs = _storage.MetadataGraphs.ToList()
            }
        };

    public RecordBuilder WithAdditionalMetadata(IEnumerable<Triple> triples) => WithAdditionalMetadata(triples.ToArray());

    public RecordBuilder WithAdditionalMetadata(params string[] rdfStrings) =>
        this with
        {
            _storage = _storage with
            {
                MetadataRdfStrings = _storage.MetadataRdfStrings.Concat(rdfStrings).ToList(),
                MetadataTriples = _storage.MetadataTriples.ToList(),
                MetadataGraphs = _storage.MetadataGraphs.ToList()
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
            ContentTriples = new(),
        }

    };


    /// <summary>
    /// Adds triples to the record content graph.
    /// Note that the triples are assumed to be in the same graph, so blank nodes are not changed.
    /// </summary>
    /// <param name="triples"></param>
    /// <returns></returns>
    public RecordBuilder WithContent(params Triple[] triples) =>
        this with
        {
            _storage = _storage with
            {
                ContentTriples = triples.ToList(),
                RdfStrings = new(),
                ContentGraphs = new()
            }
        };
    /// <summary>
    /// Adds triples to the record content graph.
    /// Note that the triples are assumed to be in the same graph, so blank nodes are not changed.
    /// </summary>
    /// <param name="triples"></param>
    /// <returns></returns>
    public RecordBuilder WithContent(IEnumerable<Triple> triples) => WithContent(triples.ToArray());
    /// <summary>
    /// Adds triples as rdf strings to the record content graph.
    /// Only triples are allowed, no named graphs in the input here.
    /// Note that the triples are assumed to be in the same graph, so blank nodes are not changed.
    /// </summary>
    /// <param name="triples"></param>
    /// <returns></returns>
    public RecordBuilder WithContent(params string[] rdfStrings) =>
        this with
        {
            _storage = _storage with
            {
                RdfStrings = rdfStrings.ToList(),
                ContentTriples = new(),
                ContentGraphs = new()
            }
        };
    public RecordBuilder WithContent(IEnumerable<string> rdfStrings) => WithContent(rdfStrings.ToArray());

    public RecordBuilder ValidateContentWithShacl(IEnumerable<string> shaclShapePaths, string describesIri, bool failOnViolation = false) =>
        this with
        {
            _shaclValidationRequest = new ShaclValidationRequest(shaclShapePaths.ToList(), describesIri, failOnViolation)
        };
    #endregion

    #region With-Additional-Content
    public RecordBuilder WithAdditionalContent(params IGraph[] graphs) =>
    this with
    {
        _storage = _storage with
        {
            ContentGraphs = _storage.ContentGraphs.Concat(graphs).ToList(),
            ContentTriples = _storage.ContentTriples.ToList(),
            RdfStrings = _storage.RdfStrings.ToList()
        }
    };

    public RecordBuilder WithAdditionalContent(IEnumerable<IGraph> graphs) => WithAdditionalContent(graphs.ToArray());

    public RecordBuilder WithAdditionalContent(params Triple[] triples) =>
        this with
        {
            _storage = _storage with
            {
                ContentTriples = _storage.ContentTriples.Concat(triples).ToList(),
                RdfStrings = _storage.RdfStrings.ToList(),
                ContentGraphs = _storage.ContentGraphs.ToList()
            }
        };
    public RecordBuilder WithAdditionalContent(IEnumerable<Triple> triples) => WithAdditionalContent(triples.ToArray());


    public RecordBuilder WithAdditionalContent(params string[] rdfStrings) =>
        this with
        {
            _storage = _storage with
            {
                RdfStrings = _storage.RdfStrings.Concat(rdfStrings).ToList(),
                ContentTriples = _storage.ContentTriples.ToList(),
                ContentGraphs = _storage.ContentGraphs.ToList()
            }
        };
    public RecordBuilder WithAdditionalContent(IEnumerable<string> rdfStrings) => WithAdditionalContent(rdfStrings.ToArray());
    #endregion
    #endregion
    #endregion

    public async Task<Record> Build()
    {
        if (_storage.Id == null) throw new RecordException("Record needs ID.");

        ValidateRecordScopeConstraint();

        var backend = await _backendFactory();

        // ── Metadata graph ──────────────────────────────────────────────────────
        var metadataTriples = CreateMetadataTriples();
        await backend.AddTriplesToGraphAsync(_storage.Id, metadataTriples);

        // In-memory metadata triples: validate then push
        var recordPredicates = GetRecordPredicates();
        if (_storage.MetadataTriples.Any(q =>
                !q.Subject.ToString().Equals(_storage.Id.ToString())
                && recordPredicates.Contains($"<{q.Predicate}>")))
            throw new RecordException(
                "For all triples where the predicate is in the record ontology, the subject must be the record itself.");
        await backend.AddTriplesToGraphAsync(_storage.Id, _storage.MetadataTriples);

        // IGraph metadata: validate then push triples into metadata named graph
        CheckMetadataGraph(recordPredicates);
        foreach (var g in _storage.MetadataGraphs)
            await backend.AddTriplesToGraphAsync(_storage.Id, g.Triples);

        // RDF-string metadata: backend parses (validated post-finalize via SPARQL)
        foreach (var s in _storage.MetadataRdfStrings)
            await backend.ParseRdfStringIntoGraphAsync(s, _storage.Id);

        // Provenance: pure computation, backend stores
        var provenanceFactory = new Graph(_storage.Id);
        await backend.AddTriplesToGraphAsync(
            _storage.Id, _metadataProvenance.Build(provenanceFactory, provenanceFactory.Name!));
        await backend.AddTriplesToGraphAsync(
            _storage.Id, _contentProvenance.Build(provenanceFactory, provenanceFactory.Name!));

        // ── Content ─────────────────────────────────────────────────────────────
        bool hasContent = _storage.ContentGraphs.Count != 0
                       || _storage.ContentTriples.Count != 0
                       || _storage.RdfStrings.Count != 0;

        if (hasContent)
        {
            var contentGraphUri = new Uri($"{_storage.Id}#content");
            var contentGraphId = new UriNode(contentGraphUri);

            await backend.AddTriplesToGraphAsync(_storage.Id, [
                new Triple(new UriNode(_storage.Id), Namespaces.Record.UriNodes.HasContent, contentGraphId)
            ]);

            // Validate in-memory content triples
            if (_storage.ContentTriples.Any(t => t.Subject.ToString().Equals(_storage.Id.ToString())))
                throw new RecordException("Content may not make metadata statements.");

            bool hasInlineContent = _storage.ContentTriples.Count != 0 || _storage.RdfStrings.Count != 0;
            if (hasInlineContent)
            {
                if (_storage.Canon == RecordCanonicalisation.dotNetRdf)
                {
                    // Canon mode: assemble the #content graph locally so we can hash it,
                    // then push the finished graph to the backend.
                    var localContent = new Graph(contentGraphUri);
                    localContent.Assert(_storage.ContentTriples);
                    foreach (var s in _storage.RdfStrings)
                    {
                        var tempStore = new TripleStore();
                        try { tempStore.LoadFromString(s); }
                        catch
                        {
                            RecordBackendBase.ValidateJsonLd(s);
                            tempStore.LoadFromString(s, new JsonLdParser());
                        }
                        if (tempStore.Graphs.Count != 1)
                            throw new RecordException("Input can only contain one graph.");
                        localContent.Assert(tempStore.Graphs.First().Triples);
                    }
                    await backend.AddGraphAsync(localContent);
                    await backend.AddTriplesToGraphAsync(_storage.Id, CreateChecksumTriples([localContent]));
                }
                else
                {
                    // No-canon path: delegate all string parsing to the backend (fast path).
                    await backend.AddTriplesToGraphAsync(contentGraphUri, _storage.ContentTriples);
                    foreach (var s in _storage.RdfStrings)
                        await backend.ParseRdfStringIntoGraphAsync(s, contentGraphUri);
                }
            }

            // Named content graphs
            CheckContentGraph();
            var userContentGraphs = _storage.ContentGraphs
                .Select(g => g.Name != null
                    ? g
                    : new Graph(new Uri($"{_storage.Id}#content{Guid.NewGuid()}"), g.Triples))
                .ToList();

            if (_storage.Canon == RecordCanonicalisation.dotNetRdf && userContentGraphs.Count != 0)
                await backend.AddTriplesToGraphAsync(_storage.Id, CreateChecksumTriples(userContentGraphs));

            foreach (var g in userContentGraphs)
            {
                await backend.AddGraphAsync(g);
                await backend.AddTriplesToGraphAsync(_storage.Id, [
                    new Triple(new UriNode(_storage.Id), Namespaces.Record.UriNodes.HasContent, g.Name)
                ]);
            }
        }

        // ── Finalise ─────────────────────────────────────────────────────────────
        await backend.FinalizeAsync();

        // Post-finalize: validate any metadata RDF strings didn't inject bad predicates
        if (_storage.MetadataRdfStrings.Count > 0)
            await ValidateMetadataRdfStrings(backend);

        var record = await Record.CreateAsync(backend, _storage.DescribesConstraintMode);

        if (_shaclValidationRequest is not null)
        {
            var outcome = await backend.ValidateContentWithShacl(
                _shaclValidationRequest.ShaclShapePaths, _shaclValidationRequest.DescribesIri);
            LastShaclValidationOutcome = outcome;

            if (!outcome.Conforms && _shaclValidationRequest.FailOnViolation)
                throw new RecordException(string.Join('\n', outcome.Messages));
        }

        return record;
    }

    /// <summary>
    /// After FinalizeAsync, verify that none of the triples loaded from metadata RDF strings
    /// use a record-ontology predicate with a subject other than the record ID.
    /// </summary>
    private async Task ValidateMetadataRdfStrings(IRecordBuildableBackend backend)
    {
        var predicatesFilter = string.Join(", ", GetRecordPredicates()); // already "<uri>" format
        var id = _storage.Id!.AbsoluteUri;

        var selectQuery =
            $"SELECT ?s WHERE {{ GRAPH <{id}> {{ " +
            $"?s ?p ?o . " +
            $"FILTER(?s != <{id}>) " +
            $"FILTER(?p IN ({predicatesFilter})) }} }} LIMIT 1";

        var results = (await backend.Sparql(selectQuery)).ToList();
        if (results.Count > 0)
            throw new RecordException(
                "For all triples where the predicate is in the record ontology, the subject must be the record itself.");
    }

    internal static IEnumerable<Triple> CreateChecksumTriples(IEnumerable<IGraph> contentGraphs)
    {
        IEnumerable<(IRefNode graphId, string value)> checkSums =
            contentGraphs.Select(g => (graphId: g.Name, value: CanonicalisationExtensions.HashGraph(g)));

        return checkSums.Select(cs =>
            {
                var graph = new Graph();
                var checkSumNode = graph.CreateBlankNode();

                graph.Assert(new Triple(cs.graphId, graph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksum)), checkSumNode));
                graph.Assert(new Triple(checkSumNode, graph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumAlgorithm)), graph.CreateUriNode(new Uri($"{Namespaces.FileContent.Spdx}checksumAlgorithm_md5"))));
                graph.Assert(new Triple(checkSumNode, graph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumValue)), graph.CreateLiteralNode(cs.value, new Uri(Namespaces.DataType.HexBinary))));

                return graph.Triples;
            }).SelectMany(g => g);
    }

    #region Private-Builder-Helper-Methods

    private void CheckMetadataGraph(IEnumerable<string> recordPredicates)
    {
        ArgumentNullException.ThrowIfNull(_storage.Id);

        foreach (var graph in _storage.MetadataGraphs.Select(g => g.Triples))
            if (graph.Any(t => !t.Subject.ToString().Equals(_storage.Id.ToString()) && recordPredicates.Contains(t.Predicate.ToString())))
                throw new RecordException("For all triples where the predicate is in the record ontology, the subject must be the record itself.");
    }

    private List<Triple> CreateMetadataTriples()
    {
        var metadataTriples = new List<Triple>();
        var typeQuad = new Triple(new UriNode(_storage.Id), new UriNode(new Uri(Namespaces.Rdf.Type)), new UriNode(new Uri(Namespaces.Record.RecordType)));
        metadataTriples.Add(typeQuad);

        if (_storage.IsSubRecordOf != null)
            metadataTriples.Add(CreateIsSubRecordOfTriple(_storage.IsSubRecordOf));

        metadataTriples.AddRange(_storage.Replaces.Select(CreateReplacesTriple).Select(q => q));
        metadataTriples.AddRange(_storage.Scopes.Select(CreateScopeTriple).Select(q => q));
        metadataTriples.AddRange(_storage.Related.Select(CreateRelatedTriple).Select(q => q));
        metadataTriples.AddRange(_storage.Describes.Select(CreateDescribesTriple).Select(q => q));

        return metadataTriples;
    }

    private void ValidateRecordScopeConstraint()
    {
        if (_storage.IsSubRecordOf == null && _storage.Scopes.Count == 0)
            throw new RecordException("A record must either be a subrecord or have at least one scope");
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
        ArgumentNullException.ThrowIfNull(_storage.Id);

        foreach (var graph in _storage.ContentGraphs.Select(g => g.Triples))
            if (graph.Any(t => t.Subject.ToString().Equals(_storage.Id.ToString())))
                throw new RecordException("Content may not make metadata statements.");
    }

    #endregion

    #region Private-Helper-Methods

    private string CreateRecordVersionUri()
    {
        var outputFolderPath = Assembly.GetExecutingAssembly()
                                   .GetManifestResourceStream("Records.Properties.commit.url") ??
                               throw new Exception("Could not get Records commit url.");
        var shapeString = new StreamReader(outputFolderPath).ReadLine() ?? throw new InvalidOperationException("Could not get Records commit url.");
        return shapeString;
    }
    private Triple CreateTripleWithPredicateAndObject(string predicate, string @object)
    {
        if (_storage.Id == null) throw new RecordException("Record ID must be added first.");
        return new Triple(new UriNode(_storage.Id), new UriNode(new Uri(predicate)), new UriNode(new Uri(@object)));
    }

    private Triple CreateIsSubRecordOfTriple(string subRecordOf) =>
        CreateTripleWithPredicateAndObject(Namespaces.Record.IsSubRecordOf, subRecordOf);

    private Triple CreateScopeTriple(string scope) =>
        CreateTripleWithPredicateAndObject(Namespaces.Record.IsInScope, scope);

    private Triple CreateRelatedTriple(string scope) =>
        CreateTripleWithPredicateAndObject(Namespaces.Record.Related, scope);

    private Triple CreateDescribesTriple(string describes) =>
        CreateTripleWithPredicateAndObject(Namespaces.Record.Describes, describes);

    private Triple CreateReplacesTriple(string replaces) =>
        CreateTripleWithPredicateAndObject(Namespaces.Record.Replaces, replaces);


    #endregion
    private sealed record ShaclValidationRequest(List<string> ShaclShapePaths, string DescribesIri, bool FailOnViolation);

#pragma warning disable IDE1006 // Naming Styles
    private record Storage
    {
        internal Uri? Id;
        internal string? IsSubRecordOf;
        internal List<string> Replaces = [];
        internal List<string> Scopes = [];
        internal List<string> Related = [];
        internal List<string> Describes = [];
        internal List<string> RdfStrings = [];

        internal List<Triple> ContentTriples = [];
        internal List<IGraph> ContentGraphs = [];
        internal List<Triple> MetadataTriples = [];
        internal List<string> MetadataRdfStrings = [];
        internal List<IGraph> MetadataGraphs = [];

        internal RecordCanonicalisation Canon = RecordCanonicalisation.None;
        internal DescribesConstraintMode DescribesConstraintMode = DescribesConstraintMode.None;
    }
#pragma warning restore IDE1006 // Naming Styles
}
