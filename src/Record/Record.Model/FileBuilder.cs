using VDS.RDF;
using AngleSharp.Dom;
using System.Security.Cryptography;
using Records.Exceptions;


namespace Records;

public record FileBuilder
{
    private IGraph _graph = new Graph();
    private Storage _storage = new();

    private record Storage
    {
        internal string? BelongsToGraph; 
        internal string? Id;
        //internal string? DownLoadUrl;
        //internal DateTime IssuedDate;
        //internal string Name;
        //internal string MediaType;
        //internal string ByteSize;
        //internal string Checksum;
        //internal string Language;
        internal IEnumerable<Quad> Quads;
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
             Quads = _storage.Quads.Append(CreateIssuedDateQuad(mediaType))
         }
     };
    public FileBuilder WithByteSize(string byteSize) =>
     this with
     {
         _storage = _storage with
         {
             Quads = _storage.Quads.Append(CreateIssuedDateQuad(byteSize))
         }
     };

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
            Quad.CreateSafe(_storage.Id!, "http://spdx.org/rdf/terms#Checksum", "_:checksum", _storage.BelongsToGraph!),
            Quad.CreateSafe("_:checksum", "http://spdx.org/rdf/terms#algorithm", ":MD5", _storage.BelongsToGraph!),
            Quad.CreateSafe("_:checksum", "http://spdx.org/rdf/terms#checksumValue", checksum, _storage.BelongsToGraph !) 
        };

    private SafeQuad CreateMediaTypeQuad(string mediaType) => Quad.CreateSafe(_storage.Id!, "http://www.w3.org/ns/dcat#mediaType", mediaType, _storage.BelongsToGraph!);

    private SafeQuad CreateByteSizeQuad( string byteSize) => Quad.CreateSafe(_storage.Id!, "http://www.w3.org/ns/dcat#byteSize", byteSize, _storage.BelongsToGraph!);

    private SafeQuad CreateFileNameQuad(string fileName) => Quad.CreateSafe(_storage.Id!, "http://purl.org/dc/terms/title", fileName, _storage.BelongsToGraph!);

    private SafeQuad CreateIssuedDateQuad(string issuedDate) => Quad.CreateSafe(_storage.Id!, "http://purl.org/dc/terms/issued", issuedDate, _storage.BelongsToGraph!);

    private SafeQuad CreateLanguageQuad(string language) => Quad.CreateSafe(_storage.Id!, "http://purl.org/dc/terms/language", language, _storage.BelongsToGraph!);

    private SafeQuad CreateDownloadUrlQuads(string downloadUrl) => Quad.CreateSafe(_storage.Id!, "http://www.w3.org/ns/dcat#downloadURL", downloadUrl, _storage.BelongsToGraph!);

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

        //TODO : add a type quad
        return recordQuads; 
    }

}
