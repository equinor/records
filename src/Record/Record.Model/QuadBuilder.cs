using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace Records;
[Obsolete("Will be removed soon. Please use DotNetRdf Graphs in stead")]
public record QuadBuilder
{
    public INode? Subject { get; private set; }
    public INode? Predicate { get; private set; }
    public INode? Object { get; private set; }
    public Uri? GraphLabel { get; private set; }

    public QuadBuilder WithSubject(string subject) =>
        this with
        {
            Subject = CreateSubject(subject)
        };

    public QuadBuilder WithPredicate(string predicate) =>
        this with
        {
            Predicate = CreatePredicate(predicate)
        };

    public QuadBuilder WithObject(string @object) =>
        this with
        {
            Object = CreateObject(@object)
        };

    public QuadBuilder WithObjectLiteral(string objectLiteral) =>
        this with
        {
            Object = CreateObjectLiteral(objectLiteral)
        };

    public QuadBuilder WithGraphLabel(string graphLabel) =>
        this with
        {
            GraphLabel = CreateGraphLabel(graphLabel)
        };

    public QuadBuilder WithTriple(Triple triple) =>
        this with
        {
            Subject = triple.Subject,
            Predicate = triple.Predicate,
            Object = triple.Object
        };
    public QuadBuilder WithStatement(string rdfString) =>
        this with
        {
            Subject = ParseTriple(rdfString).subject,
            Predicate = ParseTriple(rdfString).predicate,
            Object = ParseTriple(rdfString).@object
        };

    public SafeQuad Build()
    {
        if (GraphLabel == null) throw new QuadException("Missing graph label.");
        if (Subject == null || Predicate == null || Object == null) throw new QuadException("Missing one or more properties.");

        var tempGraph = new Graph(GraphLabel) { BaseUri = GraphLabel };

        var asserted = tempGraph.Assert(new Triple(Subject, Predicate, Object));
        if (!asserted) throw new QuadException("Failed to assert quad.");

        if (tempGraph.Triples.Count != 1) throw new QuadException("Unexpected number of triples asserted.");
        var triple = tempGraph.Triples.First();

        var formatter = new NQuadsFormatter();
        var ts = new TripleStore();
        ts.Add(tempGraph);
        var sw = new System.IO.StringWriter();
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11);
        writer.Save(ts, sw);
        var quadString = sw.ToString().Trim();

        var graphLabel = $"<{tempGraph.Name}>";


        return new SafeQuad
        {
            Subject = triple.Subject.ToString(formatter),
            Predicate = triple.Predicate.ToString(formatter),
            Object = triple.Object.ToString(formatter),
            GraphLabel = graphLabel,
            String = quadString
        };
    }

    private Uri CreateGraphLabel(string graphLabel)
    {
        try
        {
            return UriFactory.Create(graphLabel);
        }
        catch
        {
            throw new QuadException("Failed to parse graph label.");
        }
    }

    private INode? CreateSubject(string subject)
    {
        try
        {
            if (subject.StartsWith("_:")) return new BlankNode(subject); ;
            return new UriNode(UriFactory.Create(subject));
        }
        catch
        {
            throw new QuadException("Failed to parse subject.");
        }
    }

    private INode CreatePredicate(string predicate)
    {
        try
        {
            return new UriNode(UriFactory.Create(predicate));
        }
        catch
        {
            throw new QuadException("Failed to predicate.");
        }
    }

    private INode? CreateObject(string @object)
    {
        try
        {
            if (@object.StartsWith("_:")) return new BlankNode(@object);
            try
            {
                return new UriNode(UriFactory.Create(@object));
            }
            catch
            {
                return new LiteralNode(@object);
            }
        }
        catch
        {
            throw new QuadException("Failed to parse object.");
        }
    }

    private INode? CreateObjectLiteral(string objectLiteral)
    {
        try
        {
            return new LiteralNode(objectLiteral);
        }
        catch
        {
            throw new QuadException("Failed to parse object literal.");
        }
    }

    private (INode subject, INode predicate, INode @object) ParseTriple(string rdfString)
    {
        var store = new TripleStore();

        try { store.LoadFromString(rdfString); }
        catch { store.LoadFromString(rdfString, new JsonLdParser()); }

        if (store.Graphs.Count != 1) throw new QuadException("Input can only contain one triple or quad.");

        var graph = store.Graphs.First();
        if (graph.Triples.Count != 1) throw new QuadException("Input can only contain one triple or quad.");

        var triple = graph.Triples.First();

        return (triple.Subject, triple.Predicate, triple.Object);
    }
}