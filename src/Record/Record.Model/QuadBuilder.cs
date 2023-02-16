using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Records;

public record QuadBuilder
{
    private static readonly IGraph _graph = new Graph();

    public INode? Subject { get; private set; }
    public INode? Predicate { get; private set; }
    public INode? Object { get; private set; }
    public INode? GraphLabel { get; private set; }

    public IGraph Graph => _graph;

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

        var tempGraph = new Graph();
        Subject = Subject.CopyNode(tempGraph);
        Predicate = Predicate.CopyNode(tempGraph);
        Object = Object.CopyNode(tempGraph);

        var asserted = tempGraph.Assert(new Triple(Subject, Predicate, Object));
        if (!asserted) throw new QuadException("Failed to assert quad.");

        if (tempGraph.Triples.Count != 1) throw new QuadException("Unexpected number of triples asserted.");
        var triple = tempGraph.Triples.First();

        var writer = new NQuadsRecordWriter();
        var sw = new StringWriter();
        writer.Save(tempGraph, sw);
        var quadString = sw.ToString().Trim().Replace(" .", $" <{GraphLabel}> .");

        var objectString = triple.Object.ToString();

        if(triple.Object is LiteralNode literalObject)
        {
            var dataType = literalObject.DataType.ToString();
            var dataValue = literalObject.Value;

            objectString = $"\"{dataValue}\"^^<{dataType}>";
        }

        return new SafeQuad
        {
            Subject = triple.Subject.ToString(),
            Predicate = triple.Predicate.ToString(),
            Object = objectString,
            GraphLabel = GraphLabel.ToString(),
            String = quadString
        };
    }

    private INode CreateGraphLabel(string graphLabel)
    {
        try
        {
            if (graphLabel.StartsWith("_:")) return _graph.CreateBlankNode(graphLabel);
            return _graph.CreateUriNode(new Uri(graphLabel));
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
            if (subject.StartsWith("_:")) return _graph.CreateBlankNode(subject);
            return _graph.CreateUriNode(new Uri(subject));
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
            return _graph.CreateUriNode(new Uri(predicate));
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
            if (@object.StartsWith("_:")) return _graph.CreateBlankNode(@object);
            try
            {
                return _graph.CreateUriNode(new Uri(@object));
            }
            catch
            {
                return _graph.CreateLiteralNode(@object);
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
            return _graph.CreateLiteralNode(objectLiteral);
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