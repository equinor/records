using VDS.RDF;
using System.Security.Cryptography;
using Records.Exceptions;
using Microsoft.AspNetCore.Http;
using Record = Records.Immutable.Record;

namespace Records;

public record FileRecordBuilder
{
    private Storage _storage = new();
    private record Storage
    {
        internal string? Id { get; set; }
        internal string? IsSubRecordOf { get; set; }
        internal byte[]? Content { get; set; }
        internal string? FileName { get; set; }
        internal string? MediaType { get; set; }
        internal string? ByteSize { get; set; }
        internal string? Checksum { get; set; }
        internal string? Language { get; set; }
        internal List<string> Scopes = new();
    }

    #region With-Methods
    public FileRecordBuilder WithId(string id) =>
        this with
        {
            _storage = _storage with
            {
                Id = id
            }
        };
    public FileRecordBuilder WithId(Uri id) => WithId(id.ToString());
    public FileRecordBuilder WithIsSubRecordOf(string superRecordId) =>
    this with
    {
        _storage = _storage with
        {
            IsSubRecordOf = superRecordId
        }
    };
    public FileRecordBuilder WithIsSubRecordOf(Uri id) => WithIsSubRecordOf(id.ToString());
    public FileRecordBuilder WithFileContent(byte[] content) =>
    this with
    {
        _storage = _storage with
        {
            Content = content,
            Checksum = string.Join("", MD5.Create().ComputeHash(content).Select(x => x.ToString("x2"))),
            ByteSize = content.Length.ToString()
        }
    };
    public FileRecordBuilder WithFileContent(Stream content) => WithFileContent(ToByteArray(content));
    public FileRecordBuilder WithFileContent(IFormFile content) => WithFileContent(ToByteArray(content.OpenReadStream()));
    public FileRecordBuilder WithFileName(string name) =>
     this with
     {
         _storage = _storage with
         {
             FileName = name
         }
     };
    public FileRecordBuilder WithMediaType(string mediaType) =>
     this with
     {
         _storage = _storage with
         {
             MediaType = mediaType
         }
     };
    public FileRecordBuilder WithLanguage(string language) =>
     this with
     {
         _storage = _storage with
         {
             Language = language
         }
     };
    public FileRecordBuilder WithScopes(params string[] scopes) =>
    this with
    {
        _storage = _storage with
        {
            Scopes = scopes.ToList()
        }
    };
    public FileRecordBuilder WithScopes(IEnumerable<string> scopes) => WithScopes(scopes.ToArray());
    public FileRecordBuilder WithScopes(params Uri[] scopes) => WithScopes(scopes.Select(s => s.ToString()));

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

    private SafeQuad? CreateMediaTypeQuad(string? mediaType) => NullOrDo(mediaType, () => CreateQuadWithPredicateAndObject(Namespaces.FileContent.HasMediaType, $"{Namespaces.FileContent.MediaType}{mediaType}"));

    private SafeQuad? CreateByteSizeQuad(string? byteSize) => NullOrDo(byteSize, () => CreateQuadWithPredicateAndObject(Namespaces.FileContent.HasByteSize, $"{byteSize}^^{Namespaces.FileContent.Xsd}decimal"));

    private SafeQuad? CreateFileNameQuad(string? fileName) => NullOrDo(fileName, () => CreateQuadWithPredicateAndObject(Namespaces.FileContent.HasTitle, fileName));

    private SafeQuad? CreateLanguageQuad(string? language) => NullOrDo(language, () => CreateQuadWithPredicateAndObject(Namespaces.FileContent.HasLanguage, $"{language}^^{Namespaces.FileContent.Xsd}language"));

    private static SafeQuad? NullOrDo(string? @object, Func<SafeQuad> function) => string.IsNullOrWhiteSpace(@object) ? null : function();


    #endregion



    public Record Build()
    {
        VerifyBuild();

        var fileRecordQuads = new List<Quad?>
        {
            CreateMediaTypeQuad(_storage.MediaType),
            CreateByteSizeQuad(_storage.ByteSize),
            CreateFileNameQuad(_storage.FileName),
            CreateLanguageQuad(_storage.Language),
            CreateQuadWithPredicateAndObject(Namespaces.Rdf.Type, Namespaces.FileContent.Type),
            CreateQuadWithPredicateAndObject(Namespaces.FileContent.generatedAtTime, $"{DateTime.Now.Date:yyyy-MM-dd}^^{Namespaces.FileContent.Xsd}date"),
        }.Where(q => q is not null).Cast<Quad>().ToList();


        var fileRecordTriples = fileRecordQuads.Select(quad => quad.ToTriple());
        fileRecordTriples = fileRecordTriples.Concat(CreateChecksumTriples(_storage.Checksum!)!)!;

        var fileRecord = new RecordBuilder()
                             .WithId(_storage.Id)
                             .WithDescribes(_storage.Id)
                             .WithScopes(_storage.Scopes)
                             .WithIsSubRecordOf(_storage.IsSubRecordOf)
                             .WithContent(fileRecordTriples)
                             .Build();
        return fileRecord;
    }

    private void VerifyBuild()
    {
        var exceptions = new List<Exception>();

        if (_storage.Content == null) exceptions.Add(new FileRecordException("File record needs content."));
        if (_storage.FileName == null) exceptions.Add(new FileRecordException("File record needs a file name."));
        if (_storage.MediaType == null) exceptions.Add(new FileRecordException("File record needs the media type of the file."));
        if (_storage.Id == null) exceptions.Add(new FileRecordException("File record needs ID."));
        if (_storage.IsSubRecordOf == null) exceptions.Add(new FileRecordException("File record needs to have a subrecord relation."));
        if (!_storage.Scopes.Any()) exceptions.Add(new FileRecordException("File record needs scopes."));

        if (exceptions.Any())
        {
            throw exceptions.Count > 1 ? new AggregateException(exceptions) : exceptions.Single();
        }
    }

    private static byte[] ToByteArray(Stream content)
    {
        byte[] contentAsByteArray = new byte[content.Length];
        content.Read(contentAsByteArray, 0, (int)content.Length);
        return contentAsByteArray;
    }

    private SafeQuad CreateQuadWithPredicateAndObject(string predicate, string @object)
    {
        if (_storage.Id == null) throw new RecordException("Record ID must be added first.");
        return Quad.CreateSafe(_storage.Id, predicate, @object, _storage.Id);
    }

}
