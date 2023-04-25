using VDS.RDF;
using AngleSharp.Dom;
using System.Security.Cryptography;
using Records.Exceptions;
using Microsoft.AspNetCore.Http;
using VDS.RDF.Parsing;

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
    public FileBuilder WithIssuedDate(string date) =>
     this with
     {
         _storage = _storage with
         {
             IssuedDate = date
         }
     };
    public FileBuilder WithIssuedDate(DateOnly date) => WithIssuedDate(date.ToString());
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
    private List<SafeQuad?> CreateChecksumQuad(string checksum) => string.IsNullOrWhiteSpace(checksum) ? new List<SafeQuad?>() :
        new()
        {
            Quad.CreateSafe(_storage.Id!,Namespaces.FileContent.HasChecksum, "_:checksum", _storage.Id!),
            Quad.CreateSafe("_:checksum", Namespaces.FileContent.HasChecksumAlgorithm, $"{Namespaces.FileContent.Spdx}checksumAlgorithm_md5", _storage.Id!),
            Quad.CreateSafe("_:checksum", Namespaces.FileContent.HasChecksumValue, $"{checksum}^^{Namespaces.FileContent.Xsd}hexBinary" , _storage.Id!)
        };

    private SafeQuad? CreateMediaTypeQuad(string? mediaType) => NullOrDo(mediaType, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasMediaType, $"{Namespaces.FileContent.HasMediaType}{mediaType}", _storage.Id!));

    private SafeQuad? CreateByteSizeQuad(string? byteSize) => NullOrDo(byteSize, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasByteSize, $"{byteSize}^^{Namespaces.FileContent.Xsd}decimal", _storage.Id!));

    private SafeQuad? CreateFileNameQuad(string? fileName) => NullOrDo(fileName, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasTitle, fileName, _storage.Id!));

    private SafeQuad? CreateIssuedDateQuad(string? issuedDate) => NullOrDo(issuedDate, () => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.WasIssued, $"{issuedDate}^^{Namespaces.FileContent.Xsd}date", _storage.Id!));

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
        if (_storage.Id == null) throw new RecordException("File needs ID.");
        if (_storage.Content == null) throw new RecordException("File needs content");
        if (_storage.FileName == null) throw new RecordException("File needs a name.");
        if (_storage.MediaType == null) throw new RecordException("File needs a mediatype");

        var typeQuad = Quad.CreateSafe(_storage.Id, Namespaces.Rdf.Type, Namespaces.FileContent.Type, _storage.Id);
        var recordQuads = new List<Quad?>
        {
            CreateMediaTypeQuad(_storage.MediaType),
            CreateByteSizeQuad(_storage.ByteSize),
            CreateFileNameQuad(_storage.FileName),
            CreateLanguageQuad(_storage.Language),
            CreateIssuedDateQuad(_storage.IssuedDate),
            typeQuad
        }.Where(q => q is not null).Cast<Quad>().ToList();

        recordQuads.AddRange(CreateChecksumQuad(_storage.Checksum));

        return recordQuads.Select(quad => quad.ToTriple());
    }

}
