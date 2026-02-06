using System.Net.Http.Headers;
using VDS.RDF;

namespace Records;

/// <summary>
/// The valid mediatypes for writing records are trig, quads and json-ld.
/// Turtle and n-triples are not supported for writing, as they do not support named graphs, which are essential for our use case.
/// </summary>
public enum RdfMediaType
{
    Trig,
    Quads,
    JsonLd
}

public static class RdfMediaTypesExtensions
{
    public static MediaTypeHeaderValue GetMediaTypeHeaderValue(this RdfMediaType mediaType) => mediaType switch
    {
        RdfMediaType.Trig => new MediaTypeHeaderValue("application/trig"),
        RdfMediaType.Quads => new MediaTypeHeaderValue("application/quads"),
        RdfMediaType.JsonLd => new MediaTypeHeaderValue("application/ld+json"),
        _ => throw new NotSupportedException($"The media type {mediaType} is not supported.")
    };
    public static MediaTypeWithQualityHeaderValue GetMediaTypeWithQualityHeaderValue(this RdfMediaType mediaType) => mediaType switch
    {
        RdfMediaType.Trig => new MediaTypeWithQualityHeaderValue("application/trig"),
        RdfMediaType.Quads => new MediaTypeWithQualityHeaderValue("application/quads"),
        RdfMediaType.JsonLd => new MediaTypeWithQualityHeaderValue("application/ld+json"),
        _ => throw new NotSupportedException($"The media type {mediaType} is not supported.")
    };
    public static IStoreWriter GetStoreWriter(this RdfMediaType mediaType) => mediaType switch
    {
        RdfMediaType.Trig => new VDS.RDF.Writing.TriGWriter(),
        RdfMediaType.Quads => new VDS.RDF.Writing.NQuadsWriter(),
        RdfMediaType.JsonLd => new VDS.RDF.Writing.JsonLdWriter(),
        _ => throw new NotSupportedException($"The media type {mediaType} is not supported.")
    };
    public static RdfMediaType GetRdfMediaType(this IStoreWriter writer) => writer switch
    {
        VDS.RDF.Writing.TriGWriter => RdfMediaType.Trig,
        VDS.RDF.Writing.NQuadsWriter => RdfMediaType.Quads,
        VDS.RDF.Writing.JsonLdWriter => RdfMediaType.JsonLd,
        _ => throw new NotSupportedException($"The writer {writer.GetType().Name} is not supported.")
    };
}