﻿using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Shacl;
using VDS.RDF.Shacl.Validation;
using Triple = VDS.RDF.Triple;
using Record = Records.Immutable.Record;
using VDS.RDF.Writing;

namespace Records;

public record RecordBuilder
{
    private Storage _storage = new();
    private IGraph _graph;
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

        rec:ReplacedRecordShape
            a sh:NodeShape ;
            rdfs:comment """" ;
            sh:targetClass rec:ReplacedRecord ;
            sh:maxExclusive 0 ;
            sh:name ""ReplacedRecord"" ;
            sh:message ""The inferred replaced record class cannot be set explicitly. Please use the rec:Record type along side rec:replaces"" .

        rec:NewestRecordShape
            a sh:NodeShape ;
            rdfs:comment """" ;
            sh:targetClass rec:NewestRecord ;
            sh:maxExclusive 0 ;
            sh:name ""NewestRecord"" ;
            sh:message ""The inferred newest record class cannot be set explicitly. Please use the rec:Record type along side rec:replaces"" .

        rec:RecordShape
            a sh:NodeShape ;
            rdfs:comment ""This shape is for the actual transmission format of records. It should validate a single record, meaning it does not need the other records like the super record to validate. It will not validate after record-rules.ttl is applied."" ;
            sh:targetClass rec:Record ;
            sh:property [ 
                sh:path [ sh:alternativePath (rec:isSubRecordOf  rec:isInScope) ];
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
                sh:path rec:isInScopeInf;
                sh:maxCount 0;
                sh:name ""InfScope"";
                sh:message ""The inferred scope relation cannot be set explicitly. Please use rec:isInScope"";
            ] ,
            [
                sh:path rec:isInSubRecordTreeOf;
                sh:maxCount 0;
                sh:name ""SubRecordTree"";
                sh:message ""The inferred subrecord relation cannot be set explicitly. Please use rec:isSubRecordOf"";
            ] ,
            [
                sh:path rec:hasNewerSuperRecordInf;
                sh:maxCount 0;
                sh:name ""HasNewerSuperRecordInf"";
                sh:message ""The inferred subrecord relation cannot be set explicitly. Please use rec:isSubRecordOf"";
            ] ,
            [
                sh:path rec:isSubRecordOf;
                sh:minCount 0;
                sh:maxCount 1;
                sh:name ""SubRecord"";
                sh:message ""A record can be the subrecord of at most one record"";
                sh:severity sh:Violation;
            ] ,
            [
                sh:path rec:replaces;
                sh:minCount 0;
                sh:name ""Replaces"";
                sh:message ""A record replaces any number of other records. If there are none, that means the history is unknown or new. If there are more than one, this represents a merge."";
                sh:severity sh:Violation;
            ] ,
            [ 
                sh:path [ sh:inversePath rec:replaces ];
                sh:minCount 0;
                sh:name ""Replaced by"";
                sh:message ""A record can be replaced by at most one other record"";
                sh:severity sh:Violation;
            ] , 
            [
                sh:path rec:replacedBy ;
                sh:maxCount 0;
                sh:name ""Replaced by (explicit)"" ;
                sh:message ""The inferred replaced by relation cannot be set explicitly. Please use rec:replaces"";
            ],
            [
                sh:path rec:describes;
                sh:minCount 0;
                sh:name ""Describes"";
                sh:message ""A record can describe any number of objects/entities. A record describing 0 objects has no content."";
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
            ] ,
            [ 
                sh:path ([ sh:zeroOrMorePath rec:isSubRecordOf ]  rec:isInScope ) ;
                sh:minCount 1;
                sh:deactivated true;
                sh:name ""SuperRecordScope"";
                sh:message ""All records must have at least one scope, potentially reachable via subRecordOf relations"";
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
        _graph = new Graph(_storage.Id);
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

        var tripleString = string.Join("\n", recordQuads.Select(q => q.ToTripleString()));
        _graph.LoadFromString(tripleString);

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

    private (SafeQuad, INode) CreateProvenanceActivity(IGraph graph)
    {
        var activity = graph.CreateBlankNode();
        CreateQuadWithPredicateAndObject(Namespaces.Provo, replaces);
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