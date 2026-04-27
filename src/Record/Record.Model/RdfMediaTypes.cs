using System.Net.Http.Headers;
using VDS.RDF;

namespace Records;

/// <summary>
/// The valid mediatypes for writing records are trig, quads and json-ld.
/// Turtle and n-triples are not supported for writing, as they do not support named graphs, which are essential for our use case.
/// </summary>
public enum RdfMediaType
{
    Any,
    Trig,
    Quads,
    JsonLd
}

public static class RdfMediaTypesExtensions
{
    public const string NQuadsMediaType = "application/n-quads";
    public const string TriGMediaType = "application/trig";
    public const string JsonLdMediaType = "application/ld+json";
    public const string AnyMediaType = "*/*";
    public static RdfMediaType ParseMediaType(string mediaType) =>
        mediaType switch
        {
            NQuadsMediaType => RdfMediaType.Quads,
            TriGMediaType => RdfMediaType.Trig,
            JsonLdMediaType => RdfMediaType.JsonLd,
            AnyMediaType => RdfMediaType.Any,
            _ => throw new InvalidDataException($"Media-type {mediaType} not supported ")
        };
    
    public static string ToMediaTypeString(this RdfMediaType mediaType) => mediaType switch
    {
        RdfMediaType.Trig => TriGMediaType,
        RdfMediaType.Quads => NQuadsMediaType,
        RdfMediaType.JsonLd => JsonLdMediaType,
        RdfMediaType.Any => AnyMediaType,
        _ => throw new NotSupportedException($"The media type {mediaType} is not supported.")
    };
    public static MediaTypeHeaderValue GetMediaTypeHeaderValue(this RdfMediaType mediaType) => 
        new MediaTypeHeaderValue(ToMediaTypeString(mediaType));
    public static MediaTypeWithQualityHeaderValue GetMediaTypeWithQualityHeaderValue(this RdfMediaType mediaType) => 
         new MediaTypeWithQualityHeaderValue(ToMediaTypeString(mediaType));
    
    public static IStoreWriter GetStoreWriter(this RdfMediaType mediaType) => 
    MimeTypesHelper.GetStoreWriter(mediaType.ToMediaTypeString());

    public static IStoreReader GetParser(this RdfMediaType mediaType) =>
        MimeTypesHelper.GetStoreParser(mediaType.ToMediaTypeString());
    public static RdfMediaType GetRdfMediaType(this IStoreWriter writer) => writer switch
    {
        VDS.RDF.Writing.TriGWriter => RdfMediaType.Trig,
        VDS.RDF.Writing.NQuadsWriter => RdfMediaType.Quads,
        VDS.RDF.Writing.JsonLdWriter => RdfMediaType.JsonLd,
        _ => throw new NotSupportedException($"The writer {writer.GetType().Name} is not supported.")
    };
}