using VDS.RDF;

namespace Records.Backend;

/// <summary>
/// Extends <see cref="IRecordBackend"/> with mutable, build-time operations used by
/// <see cref="Records.RecordBuilder"/> to populate the backend incrementally without
/// round-tripping through an intermediate TripleStore. Once all data is pushed, call
/// <see cref="FinalizeAsync"/> to build query indices and initialise metadata.
/// </summary>
public interface IRecordBuildableBackend : IRecordBackend
{
    /// <summary>Adds a fully-formed named graph to the backend store.</summary>
    Task AddGraphAsync(IGraph graph);

    /// <summary>
    /// Creates or extends a named graph with the supplied in-memory triples.
    /// </summary>
    Task AddTriplesToGraphAsync(Uri graphName, IEnumerable<Triple> triples);

    /// <summary>
    /// Parses an RDF string (Turtle / JSON-LD auto-detected) and loads all triples
    /// into the specified named graph.
    /// </summary>
    Task ParseRdfStringIntoGraphAsync(string rdfString, Uri graphName);

    /// <summary>
    /// Called once when all data has been pushed. Builds internal query indices and
    /// initialises the metadata index so the instance can be used as a read backend.
    /// </summary>
    Task FinalizeAsync();
}
