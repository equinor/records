using VDS.RDF;
using AngleSharp.Dom;
using System.Security.Cryptography;
using Records.Exceptions;

namespace Records;

public record FileBuilder
{
    private Storage _storage = new();

    private record Storage
    {
        internal string? BelongsToGraph; 
        internal string? Id;
        internal IEnumerable<Quad> Quads = Enumerable.Empty<Quad>();
    }
    /**
     *     
    var file = new FileBuilder()
            .WithId("att:B123-EX-W-LA-XLSX")
            .WithName("B123-EX-W-LA-0001.xlsx")
            .WithCheckSum(content, Enum.MD5)
            .WithDownloadUrl(new Uri("eq:excelDownloadIri"))
            .Build();
      ==>
    
    att:B123-EX-W-LA-XLSX a att:File ;
            spdx:fileName "B123-EX-W-LA-0001.xlsx";
            spdx:checksum [
                spdx:algorithm spdx:md5 ;
                spdx:checksumValue "2"
            ] ;
            dcat:downloadUrl eq:excelDownloadIri .
}
     */

    #region With-Methods
    public FileBuilder WithNamedGraph(string namedGraphId) =>
    this with
    {
        _storage = _storage with
        {
            BelongsToGraph = namedGraphId
        }
    };

    public FileBuilder WithNamedGraph(Uri namedGraphId) => WithId(namedGraphId.ToString());

    public FileBuilder WithId(string id) =>
        this with
        {
            _storage = _storage with
            {
                Id = id
            }
        };

    public FileBuilder WithId(Uri id) => WithId(id.ToString());

    public FileBuilder WithDownloadUrl(string downloadUrl) =>
       this with
       {
           _storage = _storage with
           {
               Quads = _storage.Quads.Append(CreateDownloadUrlQuads(downloadUrl))
           }
       };

    public FileBuilder WithDownloadUrl(Url downloadUrl) => WithDownloadUrl(downloadUrl.ToString());
    public FileBuilder WithIssuedDate(string date) =>
     this with
     {
         _storage = _storage with
         {
             Quads = _storage.Quads.Append(CreateIssuedDateQuad(date))
         }
     };

    public FileBuilder WithIssuedDate(DateOnly date) => WithIssuedDate(date.ToString());

    public FileBuilder WithFileName(string name) =>
     this with
     {
         _storage = _storage with
         {
             Quads = _storage.Quads.Append(CreateFileNameQuad(name))
         }
     };
    public FileBuilder WithMediaType(string mediaType) =>
     this with
     {
         _storage = _storage with
         {
             Quads = _storage.Quads.Append(CreateMediaTypeQuad(mediaType))
         }
     };
    public FileBuilder WithByteSize(string byteSize) =>
     this with
     {
         _storage = _storage with
         {
             Quads = _storage.Quads.Append(CreateByteSizeQuad(byteSize))
         }
     };

    public FileBuilder WithByteSize(double bytes) => WithByteSize(bytes.ToString());

    public FileBuilder WithCheckSum(byte[] content) =>
        this with
        {
            _storage = _storage with
            {
                Quads = _storage.Quads.Concat(CreateChecksumQuad(string.Join("", MD5.Create().ComputeHash(content).Select(x => x.ToString("x2")))))
            }
        };
    

    public FileBuilder WithLanguage(string language) =>
     this with
     {
         _storage = _storage with
         {
             Quads = _storage.Quads.Append(CreateIssuedDateQuad(language))
         }
     };

    #endregion

    #region Create-Quads
    private List<SafeQuad> CreateChecksumQuad(string checksum) =>
        new()
        { 
            Quad.CreateSafe(_storage.Id!,Namespaces.FileRecord.HasChecksum, "_:checksum", _storage.BelongsToGraph!),
            Quad.CreateSafe("_:checksum", Namespaces.FileRecord.HasChecksumAlgorithm, $"{Namespaces.FileRecord.Spdx}checksumAlgorithm_md5", _storage.BelongsToGraph!),
            Quad.CreateSafe("_:checksum", Namespaces.FileRecord.HasChecksumValue, $"{checksum}^^{Namespaces.FileRecord.Xsd}hexBinary" , _storage.BelongsToGraph !) 
        };

    private SafeQuad CreateMediaTypeQuad(string mediaType) => Quad.CreateSafe(_storage.Id!, Namespaces.FileRecord.HasMediaType, $"{Namespaces.FileRecord.HasMediaType}{mediaType}", _storage.BelongsToGraph!);

    private SafeQuad CreateByteSizeQuad( string byteSize) => Quad.CreateSafe(_storage.Id!, Namespaces.FileRecord.HasByteSize, $"{byteSize}^^{Namespaces.FileRecord.Xsd}decimal", _storage.BelongsToGraph!);

    private SafeQuad CreateFileNameQuad(string fileName) => Quad.CreateSafe(_storage.Id!, Namespaces.FileRecord.HasTitle, fileName, _storage.BelongsToGraph!);

    private SafeQuad CreateIssuedDateQuad(string issuedDate) => Quad.CreateSafe(_storage.Id!,Namespaces.FileRecord.WasIssued, $"{issuedDate}^^{Namespaces.FileRecord.Xsd}date", _storage.BelongsToGraph!);

    private SafeQuad CreateLanguageQuad(string language) => Quad.CreateSafe(_storage.Id!, Namespaces.FileRecord.HasLanguage, $"{language}^^{Namespaces.FileRecord.Xsd}language", _storage.BelongsToGraph!);

    private SafeQuad CreateDownloadUrlQuads(string downloadUrl) => Quad.CreateSafe(_storage.Id!, Namespaces.FileRecord.HasDownloadUrl, downloadUrl, _storage.BelongsToGraph!);

    #endregion

    public IEnumerable<Quad> Build()
    {
        if (_storage.Id == null) throw new RecordException("Record needs ID.");
        if (_storage.BelongsToGraph == null) throw new RecordException("Record needs to belong to a named graph.");
        var recordQuads = new List<SafeQuad>();
        recordQuads.AddRange(_storage.Quads.Select(quad =>
        {
            return quad switch
            {
                SafeQuad safeQuad => safeQuad,
                UnsafeQuad unsafeQuad => unsafeQuad.MakeSafe(),
                _ => throw new QuadException($"You cannot use {nameof(Quad)} directly.")
            };
        }));

        var typeQuad = Quad.CreateSafe(_storage.Id, Namespaces.Rdf.Type, Namespaces.FileRecord.Type, _storage.BelongsToGraph);
        recordQuads.Add(typeQuad);  
        return recordQuads; 
    }

}
