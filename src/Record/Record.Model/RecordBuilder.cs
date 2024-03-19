﻿using System.Reflection;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using VDS.RDF.Shacl.Validation;
using Triple = VDS.RDF.Triple;
using Record = Records.Immutable.Record;
using VDS.RDF.Writing;
using Path = System.IO.Path;

namespace Records;

public record RecordBuilder
{
    private Storage _storage = new();
    private IGraph _graph;
    private ShapesGraph _processor;

    public RecordBuilder()
    {
        var shapes = new Graph();
        var outputFolderPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ??
                               throw new Exception("Cannot get absolute path");
        shapes.LoadFromFile($"{outputFolderPath}/Schema/record-single-syntax.shacl.ttl");
        _processor = new ShapesGraph(shapes);
    }

    #region With-Methods

    #region Provenance-Methods
    #region With-Provenance-Methods

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

    #region With-Additional-Provenance-Methods
    public RecordBuilder WithAdditionalScopes(params string[] scopes) =>
        this with
        {
            _storage = _storage with
            {
                Scopes = _storage.Scopes.Concat(scopes).ToList()
            }
        };

    public RecordBuilder WithAdditionalUsed(params string[] used) =>
        this with
        {
            _storage = _storage with
            {
                ProvenanceActivityUses = _storage.ProvenanceActivityUses.Concat(used).ToList()
            }
        };
    public RecordBuilder WithAdditionalWasAssociatedWith(params string[] activities) =>
        this with
        {
            _storage = _storage with
            {
                ProvenanceActivityAssociatedWith = _storage.ProvenanceActivityAssociatedWith.Concat(activities).ToList()
            }
        };

    public RecordBuilder WithLocatedAt(params string[] locations) =>
        this with
        {
            _storage = _storage with
            {
                ProvenanceActivityLocatedAt = _storage.ProvenanceActivityLocatedAt.Concat(locations).ToList()
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


    #endregion
    #endregion

    #region Content-Methods
    #region With-Content
    public RecordBuilder WithContent(params Quad[] quads) =>
        this with
        {
            _storage = _storage with
            {
                Quads = quads.ToList(),
                RdfStrings = new(),
                Triples = new()
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
                RdfStrings = new()
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
                Quads = new()
            }
        };
    public RecordBuilder WithContent(IEnumerable<string> rdfStrings) => WithContent(rdfStrings.ToArray());
    #endregion

    #region With-Additional-Content
    public RecordBuilder WithAdditionalContent(params Triple[] triples) =>
        this with
        {
            _storage = _storage with
            {
                Triples = _storage.Triples.Concat(triples).ToList(),
                Quads = _storage.Quads.ToList(),
                RdfStrings = _storage.RdfStrings.ToList()
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
                RdfStrings = _storage.RdfStrings.ToList()
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
                Triples = _storage.Triples.ToList()
            }
        };
    public RecordBuilder WithAdditionalContent(IEnumerable<string> rdfStrings) => WithAdditionalContent(rdfStrings.ToArray());
    #endregion
    #endregion
    #endregion

    public Record Build()
    {
        if (_storage.Id == null) throw new RecordException("Record needs ID.");
        _graph = new Graph(_storage.Id);
        _graph.BaseUri = _storage.Id;
        
        _storage.ProvenanceActivityAssociatedWith.Add(CreateRecordVersionUri());

        var recordQuads = new List<SafeQuad>();
        recordQuads.AddRange(_storage.Quads.Select(quad =>
        {
            return quad switch
            {
                SafeQuad safeQuad => safeQuad,
                UnsafeQuad unsafeQuad => unsafeQuad.MakeSafe(),
                _ => throw new QuadException($"You cannot use {nameof(Quad)} directly.")
            };
        }));

        recordQuads.AddRange(_storage.Triples.Select(CreateQuadFromTriple));


        var typeQuad = CreateQuadWithPredicateAndObject(Namespaces.Rdf.Type, Namespaces.Record.RecordType);
        recordQuads.Add(typeQuad);

        if (_storage.IsSubRecordOf != null)
            recordQuads.Add(CreateIsSubRecordOfQuad(_storage.IsSubRecordOf));

        recordQuads.AddRange(_storage.Replaces.Select(CreateReplacesQuad));
        recordQuads.AddRange(_storage.RdfStrings.SelectMany(SafeQuadListFromRdfString));
        recordQuads.AddRange(_storage.Scopes.Select(CreateScopeQuad));
        recordQuads.AddRange(_storage.Describes.Select(CreateDescribesQuad));

        var tripleString = string.Join("\n", recordQuads.Select(q => q.ToTripleString()));
        _graph.LoadFromString(tripleString);
        _graph.Assert(CreateProvenanceActivity(_graph));
        var report = _processor.Validate(_graph);
        if (!report.Conforms) throw ShaclException(report);

        var writer = new NQuadsWriter();
        var sw = new System.IO.StringWriter();
        var ts = new TripleStore();
        ts.Add(_graph);
        writer.Save(ts, sw);

        return new(sw.ToString());
    }

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

        return tempStore.Graphs.First().Triples.Select(triple => Quad.CreateSafe(triple, _graph.BaseUri)).ToList();
    }

    private string CreateRecordVersionUri() =>
        $"https://www.nuget.org/packages/Record/{GetType().Assembly.GetName().Version}";



    private SafeQuad CreateQuadWithPredicateAndObject(string predicate, string @object)
    {
        if (_graph.BaseUri == null) throw new RecordException("Record ID must be added first.");
        return Quad.CreateSafe(_graph.BaseUri.ToString(), predicate, @object, _graph.BaseUri.ToString());
    }

    private SafeQuad CreateQuadFromTriple(Triple triple)
    {
        return Quad.CreateSafe(triple, _graph.BaseUri.ToString());
    }

    private SafeQuad CreateIsSubRecordOfQuad(string subRecordOf) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.IsSubRecordOf, subRecordOf);

    private SafeQuad CreateScopeQuad(string scope) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.IsInScope, scope);

    private SafeQuad CreateDescribesQuad(string describes) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.Describes, describes);

    private SafeQuad CreateReplacesQuad(string replaces) =>
        CreateQuadWithPredicateAndObject(Namespaces.Record.Replaces, replaces);

    private IEnumerable<Triple> CreateProvenanceTriples(INodeFactory graph, IRefNode activity, List<string> provenanceObjects,
        Uri property) =>
        provenanceObjects.Select(used =>
            new Triple(
                activity,
                graph.CreateUriNode(property),
                graph.CreateUriNode(new Uri(used))
            ));

    private List<Triple> CreateProvenanceActivity(IGraph graph)
    {
        var provenanceTriples = new List<Triple>();
        var activity = graph.CreateBlankNode();
        var recordNode = graph.Name;
        provenanceTriples.Add(
            new Triple(
                recordNode,
                graph.CreateUriNode(new Uri(Namespaces.Prov.WasGeneratedBy)),
                activity)
            );
        foreach (var (objectList, property) in new (List<string>, Uri)[]
                 {
                     (_storage.ProvenanceActivityUses, new Uri(Namespaces.Prov.WasGeneratedBy)),
                     (_storage.ProvenanceActivityAssociatedWith, new Uri(Namespaces.Prov.WasAssociatedWith)),
                     (_storage.ProvenanceActivityLocatedAt, new Uri(Namespaces.Prov.AtLocation))
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
    #endregion

    private record Storage
    {
        internal Uri? Id;
        internal string? IsSubRecordOf;
        internal List<string> Replaces = new();
        internal List<string> Scopes = new();
        internal List<string> Describes = new();
        internal List<string> RdfStrings = new();
        internal List<string> ProvenanceActivityUses = new();
        internal List<string> ProvenanceActivityAssociatedWith = new();
        internal List<string> ProvenanceActivityLocatedAt = new();

        internal List<Quad> Quads = new();
        internal List<Triple> Triples = new();
    }
}