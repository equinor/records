﻿using System.Diagnostics;

namespace Records.Mutable;

[Obsolete("Mutable.Record will no longer be updated or maintained. We suggest you utilise IGraphs when you need to alter RDF content. You can collect a record's IGraph using Immutable.Record.Graph().")]
[DebuggerDisplay($"{{{nameof(Id)}}}")]
public record Record(string Id)
{
    public string Id { get; private init; } = Id;
    public List<string> QuadStrings { get; set; } = new() { $"<{Id}> <{Namespaces.Rdf.Type}> <{Namespaces.Record.RecordType}> <{Id}> ." };

    public Record(Immutable.Record record) : this(record.Id)
    {
        QuadStrings = record.ToString()
            .Split("\n")
            .Where(line => !string.IsNullOrEmpty(line))
            .ToList();
    }

    public Immutable.Record ToImmutable()
    {
        var rdf = String.Join("", QuadStrings
            .Where(quad => !string.IsNullOrEmpty(quad)));
        return new Immutable.Record(rdf);
    }

    /// <summary>
    /// Replaces is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/replaces/original"
    /// </code>
    /// </example>
    /// </summary>
    public Record WithAdditionalReplaces(params string[] replaces) =>
        this with
        {
            QuadStrings = QuadStrings
                .Concat(replaces.Select(ReplacesQuad))
                .ToList()
        };

    /// <summary>
    /// Replaces is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/replaces/original"
    /// </code>
    /// </example>
    /// </summary>
    public void AddReplaces(params string[] replaces)
    {
        QuadStrings.AddRange(replaces.Select(ReplacesQuad));
    }

    /// <summary>
    /// Scopes is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/scope/example"
    /// </code>
    /// </example>
    /// </summary>
    public Record WithAdditionalScopes(params string[] scopes) =>
        this with
        {
            QuadStrings = QuadStrings
                .Concat(scopes.Select(ScopeQuad))
                .ToList()
        };

    /// <summary>
    /// Scopes is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/scope/example"
    /// </code>
    /// </example>
    /// </summary>
    public void AddScopes(params string[] scopes)
    {
        QuadStrings.AddRange(scopes.Select(ScopeQuad));
    }

    /// <summary>
    /// Describes is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/object/example"
    /// </code>
    /// </example>
    /// </summary>
    public Record WithAdditionalDescribes(params string[] describes) =>
        this with
        {
            QuadStrings = QuadStrings
                .Concat(describes.Select(DescribesQuad))
                .ToList()
        };

    /// <summary>
    /// Describes is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/object/example"
    /// </code>
    /// </example>
    /// </summary>
    public void AddDescribes(params string[] describes)
    {
        QuadStrings.AddRange(describes.Select(DescribesQuad));
    }

    /// <summary>
    /// IsSubRecordOf is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/record/example"
    /// </code>
    /// </example>
    /// Note: This method does not check if there already exists a rec:isSubRecordOf relation.
    /// </summary>
    public Record WithIsSubRecordof(string isSubRecordof) =>
        this with
        {
            QuadStrings = QuadStrings
                .Concat(new[] { SubRecordQuad(isSubRecordof) })
                .ToList()
        };

    /// <summary>
    /// IsSubRecordOf is assumed to be a serialized IRI / URI without the angle brackets.
    /// <example>
    /// Examples:
    /// <code>
    /// "https://example.com/record/example"
    /// </code>
    /// </example>
    /// Note: This method does not check if there already exists a rec:isSubRecordOf relation.
    /// </summary>
    public void AddIsSubRecordOf(string isSubRecordof)
    {
        QuadStrings.Add(SubRecordQuad(isSubRecordof));
    }

    /// <summary>
    /// Input here is assumed to follow the NQuads format for RDF. Each string is one quad.
    /// Find format at <seealso href="https://www.w3.org/TR/n-quads/#sec-grammar">W3.org</seealso>
    /// </summary>
    public Record WithAdditionalQuads(params string[] quads) =>
        this with
        {
            QuadStrings = QuadStrings.Concat(quads).ToList()
        };

    /// <summary>
    /// Input here is assumed to follow the NQuads format for RDF. Each string is one quad.
    /// Find format at <seealso href="https://www.w3.org/TR/n-quads/#sec-grammar">W3.org</seealso>
    /// </summary>
    public void AddQuads(params string[] quads)
    {
        QuadStrings.AddRange(quads);
    }

    /// <summary>
    /// Input here is assumed to follow the NTriples format for RDF. Each string is one triple.
    /// This method adds on the graph label after the object.
    /// Find format at <seealso href="https://www.w3.org/TR/n-triples/#n-triples-grammar">W3.org</seealso>
    /// </summary>
    public Record WithAdditionalTriples(params string[] triples) =>
        this with
        {
            QuadStrings = QuadStrings
                .Concat(triples
                    .Select(t =>
                    {
                        var lastPeriod = t.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                        return t[..lastPeriod] + $"<{Id}> .";
                    })
                ).ToList()
        };

    /// <summary>
    /// Input here is assumed to follow the NTriples format for RDF. Each string is one triple.
    /// This method adds on the graph label after the object.
    /// Find format at <seealso href="https://www.w3.org/TR/n-triples/#n-triples-grammar">W3.org</seealso>
    /// </summary>
    public void AddTriples(params string[] triples)
    {
        QuadStrings
            .AddRange(triples
                .Select(t =>
                {
                    var lastPeriod = t.LastIndexOf(".", StringComparison.InvariantCultureIgnoreCase);
                    return t[..lastPeriod] + $"<{Id}> .";
                })
            );
    }

    public Record WithAdditionalQuads(params SafeQuad[] quads) =>
        this with
        {
            QuadStrings = QuadStrings.Concat(quads.Select(q => q.ToString())).ToList()
        };

    public void AddQuads(params SafeQuad[] quads)
    {
        QuadStrings.AddRange(quads.Select(q => q.ToString()));
    }

    private string SubRecordQuad(string subRecordof) => $"<{Id}> <{Namespaces.Record.IsSubRecordOf}> <{subRecordof}> <{Id}> .";
    private string ReplacesQuad(string replaces) => $"<{Id}> <{Namespaces.Record.Replaces}> <{replaces}> <{Id}> .";
    private string ScopeQuad(string scope) => $"<{Id}> <{Namespaces.Record.IsInScope}> <{scope}> <{Id}>.";
    private string DescribesQuad(string describes) => $"<{Id}> <{Namespaces.Record.Describes}> <{describes}> <{Id}> .";
}