using VDS.RDF;
using System.Security.Cryptography;
using Records.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Records;

public record FileBuilder
{
    private Storage _storage = new();

    private record Storage
    {
        internal string? Id { get; set; }
        internal byte[]? Content { get; set; }
        internal string? IssuedDate { get; set; }
        internal string? FileName { get; set; }
        internal string? MediaType { get; set; }
        internal string? ByteSize { get; set; }
        internal string? Checksum { get; set; }
        internal string? Language { get; set; }
    }

    #region With-Methods
    public FileBuilder WithId(string id) =>
        this with
        {
            _storage = _storage with
            {
                Id = id
            }
        };
    public FileBuilder WithId(Uri id) => WithId(id.ToString());
    public FileBuilder WithContent(byte[] content) =>
    this with
    {
        _storage = _storage with
        {
            Content = content,
            Checksum = string.Join("", MD5.Create().ComputeHash(content).Select(x => x.ToString("x2"))),
            ByteSize = content.Length.ToString()
        }
    };
    public FileBuilder WithContent(Stream content) => WithContent(ToByteArray(content));
    public FileBuilder WithContent(IFormFile content) => WithContent(ToByteArray(content.OpenReadStream()));
    public FileBuilder WithFileName(string name) =>
     this with
     {
         _storage = _storage with
         {
             FileName = name
         }
     };
    public FileBuilder WithMediaType(string mediaType) =>
     this with
     {
         _storage = _storage with
         {
             MediaType = mediaType
         }
     };
    public FileBuilder WithLanguage(string language) =>
     this with
     {
         _storage = _storage with
         {
             Language = language
         }
     };

    #endregion

    #region Create-Quads
    private IEnumerable<Triple?> CreateChecksumTriples(string checksum)
    {
        var checksumGraph = new Graph();
        var checksumBlank = checksumGraph.CreateBlankNode();
        checksumGraph.Assert(new Triple(checksumGraph.CreateUriNode(new Uri(_storage.Id!)), checksumGraph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksum)), checksumBlank));
        checksumGraph.Assert(new Triple(checksumBlank, checksumGraph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumAlgorithm)), checksumGraph.CreateUriNode(new Uri($"{Namespaces.FileContent.Spdx}checksumAlgorithm_md5"))));
        checksumGraph.Assert(new Triple(checksumBlank, checksumGraph.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumValue)), checksumGraph.CreateLiteralNode(checksum, new Uri($"{Namespaces.FileContent.Xsd}hexBinary"))));
        return checksumGraph.Triples;
    }

    private SafeQuad? CreateMediaTypeQuad(string? mediaType) => NullOrDo(mediaType, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasMediaType, $"{Namespaces.FileContent.MediaType}{mediaType}", _storage.Id!));

    private SafeQuad? CreateByteSizeQuad(string? byteSize) => NullOrDo(byteSize, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasByteSize, $"{byteSize}^^{Namespaces.FileContent.Xsd}decimal", _storage.Id!));

    private SafeQuad? CreateFileNameQuad(string? fileName) => NullOrDo(fileName, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasTitle, fileName, _storage.Id!));

    private SafeQuad? CreateLanguageQuad(string? language) => NullOrDo(language, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasLanguage, $"{language}^^{Namespaces.FileContent.Xsd}language", _storage.Id!));

    private static SafeQuad? NullOrDo(string? @object, Func<SafeQuad> function) => string.IsNullOrWhiteSpace(@object) ? null : function();


    #endregion
    private static byte[] ToByteArray(Stream content)
    {
        byte[] contentAsByteArray = new byte[content.Length];
        content.Read(contentAsByteArray, 0, (int)content.Length);
        return contentAsByteArray;
    }

    public IEnumerable<Triple> Build()
    {
        if (_storage.Id == null) throw new RecordException("File needs the ID from the record of which it will be content for.");
        if (_storage.Content == null) throw new FileException("File needs content");
        if (_storage.FileName == null) throw new FileException("File needs a name.");
        if (_storage.MediaType == null) throw new FileException("File needs a mediatype");

        var typeQuad = Quad.CreateSafe(_storage.Id, Namespaces.Rdf.Type, Namespaces.FileContent.Type, _storage.Id);
        var recordQuads = new List<Quad?>
        {
            CreateMediaTypeQuad(_storage.MediaType),
            CreateByteSizeQuad(_storage.ByteSize),
            CreateFileNameQuad(_storage.FileName),
            CreateLanguageQuad(_storage.Language),
            typeQuad
        }.Where(q => q is not null).Cast<Quad>().ToList();

        var recordTriples = recordQuads.Select(quad => quad.ToTriple());
        recordTriples = recordTriples.Concat(CreateChecksumTriples(_storage.Checksum));

        return recordTriples;
    }

}
