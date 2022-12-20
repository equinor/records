using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using VDS.RDF.Shacl.Validation;
using Triple = VDS.RDF.Triple;
using Record = Records.Immutable.Record;

namespace Records;

public record RecordBuilder
{
    private Storage _storage = new();
    private IGraph _graph = new Graph();
    private ShapesGraph _processor;

    public RecordBuilder()
    {
        var shapes = new Graph();
        shapes.LoadFromString(@"
@prefix sh: <http://www.w3.org/ns/shacl#>.
@prefix rec: <https://rdf.equinor.com/ontology/record/>.
@prefix rdfs: 	<http://www.w3.org/2000/01/rdf-schema#>.
@prefix lis: <http://standards.iso.org/iso/15926/part14/>.
@prefix skos: <http://www.w3.org/2004/02/skos/core#> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

rec:RecordShape
    a sh:NodeShape ;
    sh:targetClass rec:Record ;
    sh:property [ 
        sh:path [ sh:alternativePath (rec:isSubrecordOf  rec:isInScope) ];
        sh:minCount 1;
        sh:name ""Scope"";
        sh:message ""A record must either be a subrecord or have at least one scope"";
        sh:severity sh:Violation;
    ] , 
    [
        sh:path rec:isInScope;
        sh:minCount 0;
        sh:name ""Scope"";
        sh:message ""A record can have any number of scopes set directly"";
    ] ,
    [
        sh:path rec:isSubrecordOf;
        sh:class rec:Record;
        sh:minCount 0;
        sh:maxCount 1;
        sh:name ""SubRecord"";
        sh:message ""A record can be the subrecord of at most one record"";
        sh:severity sh:Violation;
    ] ,
    [
        sh:path rec:describes;
        sh:minCount 0;
        sh:name ""Describes"";
        sh:message ""A record can describe any number of objects/entities"";
    ]  ,
    [
        sh:path rdfs:comment;
        sh:name ""Comment"";
        sh:datatype xsd:string ;
        sh:maxCount 1;
        sh:minCount 0 ;
        sh:message ""A record can have at most one comment"";
        sh:severity sh:Warning;
    ] ,
    [ 
        sh:datatype xsd:string ;
        sh:maxCount 1 ;
        sh:minCount 0 ;
        sh:path skos:prefLabel ;
        sh:message ""A record can have at most one skos:prefLabel"";
        sh:severity sh:Warning 
    ],
    [
        sh:path rdfs:label;
        sh:name ""Label"";
        sh:maxCount 1;
        sh:message ""A record can have at most one label"";
        sh:severity sh:Warning;
    ] .
    ");
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
        _graph.BaseUri = _storage.Id;

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

        foreach (var quad in recordQuads) _graph.LoadFromString(quad.ToTripleString());

        var report = _processor.Validate(_graph);
        if (!report.Conforms) throw ShaclException(report);

        var writer = new NQuadsRecordWriter();
        var sw = new StringWriter();
        writer.Save(_graph, sw);

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
            .Select(t => t.Object.ToString())
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

    private SafeQuad CreateQuadWithPredicateAndObject(string predicate, string @object)
    {
        if (_graph.BaseUri == null) throw new RecordException("Record ID must be added first.");
        return Quad.CreateSafe(_graph.BaseUri.ToString(), predicate, @object, _graph.BaseUri.ToString());
    }

    private SafeQuad CreateQuadFromTriple(Triple triple)
    {
        return Quad.CreateSafe(triple.Subject.ToString(), triple.Predicate.ToString(), triple.Object.ToString(), _graph.BaseUri.ToString());
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
    }
}