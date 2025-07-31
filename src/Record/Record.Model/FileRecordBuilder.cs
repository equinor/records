using IriTools;
using Microsoft.AspNetCore.Http;
using Records.Exceptions;
using Records.Utils;
using System.Security.Cryptography;
using VDS.RDF;
using Record = Records.Immutable.Record;

namespace Records;

public record FileRecordBuilder
{
    private Storage _storage = new();

    private record Storage
    {
        internal IriReference? Id { get; set; }
        internal IriReference? FileId { get; set; }
        internal string? IsSubRecordOf { get; set; }
        internal byte[]? Content { get; set; }
        internal string? FileName { get; set; }
        internal IriReference? DerivedFrom { get; set; }
        internal string? FileExtension { get; set; }
        internal string? ByteSize { get; set; }
        internal string? Checksum { get; set; }
        internal string? Language { get; set; }
        internal string? ModelType { get; set; }
        internal string? DocumentType { get; set; }

        internal List<string> Scopes = new();
    }

    #region With-Methods
    public FileRecordBuilder WithId(IriReference id) =>
        this with
        {
            _storage = _storage with
            {
                Id = id
            }
        };
    public FileRecordBuilder WithId(string id) => WithId(new IriReference(id));

    public FileRecordBuilder WithFileId(IriReference fileId) =>
    this with
    {
        _storage = _storage with
        {
            FileId = fileId
        }
    };
    public FileRecordBuilder WithFileId(string fileId) => WithId(new IriReference(fileId));

    public FileRecordBuilder WithDerivedFrom(IriReference id) =>
        this with
        {
            _storage = _storage with
            {
                DerivedFrom = id
            }
        };
    public FileRecordBuilder WithDerivedFrom(string id) => WithDerivedFrom(new IriReference(id));
    public FileRecordBuilder WithModelType(string modelType) =>
        this with
        {
            _storage = _storage with
            {
                ModelType = modelType
            }
        };

    public FileRecordBuilder WithDocumentType(string documentType) =>
    this with
    {
        _storage = _storage with
        {
            DocumentType = documentType
        }
    };
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
            Checksum = string.Join("", MD5.HashData(content).Select(x => x.ToString("x2"))),
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
    public FileRecordBuilder WithFileExtension(string fileExtension) =>
     this with
     {
         _storage = _storage with
         {
             FileExtension = fileExtension
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
        var content = new Graph();
        var checksumBlank = content.CreateBlankNode();
        content.Assert(new Triple(content.CreateUriNode(_storage.FileId!), content.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksum)), checksumBlank));
        content.Assert(new Triple(checksumBlank, content.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumAlgorithm)), content.CreateUriNode(new Uri($"{Namespaces.FileContent.Spdx}checksumAlgorithm_md5"))));
        content.Assert(new Triple(checksumBlank, content.CreateUriNode(new Uri(Namespaces.FileContent.HasChecksumValue)), content.CreateLiteralNode(checksum, new Uri(Namespaces.DataType.HexBinary))));
        return content.Triples;
    }

    private Triple? CreateModelTypeTriple(string? modelType) => NullOrDo(modelType, () => CreateTripleWithPredicateAndObject(Namespaces.FileContent.ModelType, modelType!, Namespaces.DataType.String));
    private Triple? CreateDocumentTypeTriple(string? documentType) => NullOrDo(documentType, () => CreateTripleWithPredicateAndObject(Namespaces.FileContent.DocumentType, documentType!, Namespaces.DataType.String));

    private Triple? CreateFileExtensionTriple(string? fileExtension) => NullOrDo(fileExtension, () => CreateTripleWithPredicateAndObject(Namespaces.FileContent.FileExtension, fileExtension!, Namespaces.DataType.String));

    private Triple? CreateByteSizeTriple(string? byteSize) => NullOrDo(byteSize, () => CreateTripleWithPredicateAndObject(Namespaces.FileContent.HasByteSize, byteSize!, Namespaces.DataType.Decimal));

    private Triple? CreateFileNameTriple(string? fileName) => NullOrDo(fileName, () => CreateTripleWithPredicateAndObject(Namespaces.FileContent.HasTitle, fileName!, Namespaces.DataType.String));

    private Triple? CreateLanguageTriple(string? language) => NullOrDo(language, () => CreateTripleWithPredicateAndObject(Namespaces.FileContent.HasLanguage, language!, Namespaces.DataType.Language));

    private static Triple? NullOrDo(string? @object, Func<Triple> function) => string.IsNullOrWhiteSpace(@object) ? null : function();


    #endregion

    public Record Build()
    {
        _storage.FileId ??= new Uri($"{Namespaces.FileContent.Att}/file/{Guid.NewGuid()}");

        VerifyBuild();

        var fileRecordTriples = new List<Triple?>
        {
            CreateLanguageTriple(_storage.Language),
            CreateByteSizeTriple(_storage.ByteSize),
            CreateFileExtensionTriple(_storage.FileExtension),
            CreateFileNameTriple(_storage.FileName),
            CreateModelTypeTriple(_storage.ModelType),
            CreateDocumentTypeTriple(_storage.DocumentType),
            CreateTripleWithPredicateAndObject(Namespaces.Rdf.Type, Namespaces.FileContent.Type),
            CreateTripleWithPredicateAndObject(Namespaces.FileContent.generatedAtTime, $"{DateTime.Now.Date:yyyy-MM-dd}", Namespaces.DataType.Date),
        }.Where(t => t is not null);

        fileRecordTriples = fileRecordTriples.Concat(CreateChecksumTriples(_storage.Checksum!));
        fileRecordTriples = fileRecordTriples.Concat(TimeUtils.CreateHasTimeTriples(new UriNode(_storage.FileId), DateTime.Now.Date));
        
        var fileRecord = new RecordBuilder()
                             .WithId(_storage.Id!)
                             .WithDescribes(_storage.FileId)
                             .WithScopes(_storage.Scopes)
                             .WithIsSubRecordOf(_storage.IsSubRecordOf!)
                             .WithContent(fileRecordTriples!);

        if (_storage.DerivedFrom != null)
        {
            fileRecord = fileRecord
                .WithAdditionalDescribes(_storage.DerivedFrom)
                .WithAdditionalContent(new Triple(new UriNode(_storage.FileId), Namespaces.Prov.UriNodes.WasDerivedFrom, new UriNode(_storage.DerivedFrom)));
        }

        return fileRecord.Build(); ;
    }

    private void VerifyBuild()
    {
        var exceptions = new List<Exception>();

        if (_storage.ModelType == null) exceptions.Add(new FileRecordException("File record needs model type."));
        if (_storage.Content == null) exceptions.Add(new FileRecordException("File record needs content."));
        if (_storage.FileName == null) exceptions.Add(new FileRecordException("File record needs a file name."));
        if (_storage.FileExtension == null) exceptions.Add(new FileRecordException("File record needs the file extension."));
        if (_storage.DocumentType == null) exceptions.Add(new FileRecordException("File record needs the document type of the file."));
        if (_storage.Id == null) exceptions.Add(new FileRecordException("File record needs ID."));
        if (!_storage.Scopes.Any()) exceptions.Add(new FileRecordException("File record needs scopes."));

        if (exceptions.Any())
        {
            throw exceptions.Count > 1 ? new AggregateException(exceptions) : exceptions.Single();
        }
    }

    private static byte[] ToByteArray(Stream content)
    {
        byte[] contentAsByteArray = new byte[content.Length];
        content.ReadExactly(contentAsByteArray, 0, (int)content.Length);
        return contentAsByteArray;
    }

    private Triple CreateTripleWithPredicateAndObject(string predicate, string @object, string literalNodeDataType = "")
    {
        ArgumentNullException.ThrowIfNull(_storage.FileId);

        return new(new UriNode(_storage.FileId),
                    new UriNode(new Uri(predicate)),
                    string.IsNullOrEmpty(literalNodeDataType) ?
                        new UriNode(new Uri(@object)) :
                        new LiteralNode(@object, new Uri(literalNodeDataType)));
    }

}

