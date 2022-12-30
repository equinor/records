using VDS.RDF;

namespace Records;

public abstract class Quad : IEquatable<Quad>
{
    public string Subject { get; internal init; } = null!;
    public string Predicate { get; internal init; } = null!;
    public string Object { get; internal init; } = null!;
    public string GraphLabel { get; internal init; } = null!;

    internal string String = null!;

    public static QuadBuilder CreateBuilder() => new();
    public static QuadBuilder CreateBuilder(string id) => new QuadBuilder().WithGraphLabel(id);
    public static QuadBuilder CreateBuilder(Uri id) => new QuadBuilder().WithGraphLabel(id.ToString());

    public static SafeQuad CreateSafe(string s, string p, string o, string g, bool objectLiteral = false)
    {
        var builder = CreateBuilder()
            .WithSubject(s)
            .WithPredicate(p)
            .WithGraphLabel(g);

        if (objectLiteral) builder = builder.WithObjectLiteral(o);
        else builder = builder.WithObject(o);

        return builder.Build();
    }

    public static SafeQuad CreateSafe(Triple triple, string g)
    {
        return CreateBuilder()
            .WithTriple(triple)
            .WithGraphLabel(g)
            .Build();
    }

    public static SafeQuad CreateSafe(Triple triple, Uri g) => CreateSafe(triple, g.ToString());

    public static UnsafeQuad CreateUnsafe(string s, string p, string o, string g)
    {
        return new UnsafeQuad
        {
            Subject = s,
            Predicate = p,
            Object = o,
            GraphLabel = g,
            String = $"{s} {p} {o} {g} ."
        };
    }

    public static Quad CreateUnsafe(Triple triple, string g)
    {
        return new UnsafeQuad()
        {
            Subject = triple.Subject.ToString(),
            Predicate = triple.Predicate.ToString(),
            Object = triple.Object.ToString(),
            GraphLabel = g,
            String = $"{triple.Subject} {triple.Predicate} {triple.Object} {g} ."
        };
    }

    public static Quad CreateUnsafe(Triple triple, Uri g) => CreateUnsafe(triple, g.ToString());

    public Triple ToTriple()
    {
        var builder = CreateBuilder()
            .WithSubject(Subject)
            .WithObject(Object)
            .WithPredicate(Predicate);

        return new Triple(builder.Subject, builder.Predicate, builder.Object, builder.Graph);
    }

    public override string ToString() => String;

    public void Deconstruct(out string s, out string p, out string o, out string g) =>
        (s, p, o, g) = (Subject, Predicate, Object, GraphLabel);

    public bool Equals(Quad? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Subject == other.Subject && Predicate == other.Predicate && Object == other.Object && GraphLabel == other.GraphLabel;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((Quad)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Subject, Predicate, Object, GraphLabel);
    }
}

/// <summary>
/// An UnsafeQuad does not do any checks on its content. It accepts any strings.
/// You may use this if you know that your original data already is correct RDF,
/// and therefore can omit the expensive validity checks.
/// </summary>
public sealed class UnsafeQuad : Quad
{
    public UnsafeQuad WithSubject(string subject)
    {
        return CreateUnsafe(subject, Predicate, Object, GraphLabel);
    }

    public UnsafeQuad WithPredicate(string predicate)
    {
        return CreateUnsafe(Subject, predicate, Object, GraphLabel);
    }

    public UnsafeQuad WithObject(string @object)
    {
        return CreateUnsafe(Subject, Predicate, @object, GraphLabel);
    }

    public UnsafeQuad WithGraphLabel(string graphLabel)
    {
        return CreateUnsafe(Subject, Predicate, Object, graphLabel);
    }

    public SafeQuad MakeSafe()
    {
        return CreateBuilder()
            .WithSubject(Subject)
            .WithPredicate(Predicate)
            .WithObject(Object)
            .WithGraphLabel(GraphLabel)
            .Build();
    }
}

/// <summary>
/// A SafeQuad has its content validated against a graph.
/// It guarantees that the content is valid RDF and can be asserted into a triple store.
/// </summary>
public class SafeQuad : Quad
{
    public SafeQuad WithSubject(string subject)
    {
        return CreateSafe(subject, Predicate, Object, GraphLabel);
    }

    public SafeQuad WithPredicate(string predicate)
    {
        return CreateSafe(Subject, predicate, Object, GraphLabel);
    }

    public SafeQuad WithObject(string @object)
    {
        return CreateSafe(Subject, Predicate, @object, GraphLabel);
    }

    public SafeQuad WithGraphLabel(string graphLabel)
    {
        return CreateSafe(Subject, Predicate, Object, graphLabel);
    }

    public string ToTripleString() => String[..String.LastIndexOf('<')] + " .";
}