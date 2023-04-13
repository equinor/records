using VDS.RDF;
using AngleSharp.Dom;
using System.Security.Cryptography;
using Records.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Records;

public record FileBuilder
{
    private Storage _storage = new();
    internal string? _memberOfNamedGraph = "rec:123.0";

    private record Storage
    {
        internal string? Id;
        internal string? DownLoadUrl;
        internal byte[]? Content;
        internal string? IssuedDate;
        internal string? FileName;
        internal string? MediaType;
        internal string? ByteSize;
        internal string? Checksum;
        internal string? Language;
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
    public FileBuilder WithId(string id) =>
        this with
        {
            _storage = _storage with
            {
                Id = id
            }
        };
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
    public FileBuilder WithId(Uri id) => WithId(id.ToString());
    public FileBuilder WithDownloadUrl(string downloadUrl) =>
       this with
       {
           _storage = _storage with
           {
               DownLoadUrl = downloadUrl
           }
       };
    public FileBuilder WithDownloadUrl(Url downloadUrl) => WithDownloadUrl(downloadUrl.ToString());
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
    private List<SafeQuad> CreateChecksumQuad(string checksum) =>
        new()
        {
            Quad.CreateSafe(_storage.Id!,Namespaces.FileContent.HasChecksum, "_:checksum", _memberOfNamedGraph!),
            Quad.CreateSafe("_:checksum", Namespaces.FileContent.HasChecksumAlgorithm, $"{Namespaces.FileContent.Spdx}checksumAlgorithm_md5", _memberOfNamedGraph!),
            Quad.CreateSafe("_:checksum", Namespaces.FileContent.HasChecksumValue, $"{checksum}^^{Namespaces.FileContent.Xsd}hexBinary" , _memberOfNamedGraph!)
        };

    private SafeQuad CreateMediaTypeQuad(string mediaType) => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasMediaType, $"{Namespaces.FileContent.HasMediaType}{mediaType}", _memberOfNamedGraph!);

    private SafeQuad CreateByteSizeQuad(string byteSize) => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasByteSize, $"{byteSize}^^{Namespaces.FileContent.Xsd}decimal", _memberOfNamedGraph!);

    private SafeQuad CreateFileNameQuad(string fileName) => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasTitle, fileName, _memberOfNamedGraph!);

    private SafeQuad CreateIssuedDateQuad(string issuedDate) => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.WasIssued, $"{issuedDate}^^{Namespaces.FileContent.Xsd}date", _memberOfNamedGraph!);

    private SafeQuad CreateLanguageQuad(string language) => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasLanguage, $"{language}^^{Namespaces.FileContent.Xsd}language", _memberOfNamedGraph!);

    private SafeQuad CreateDownloadUrlQuads(string downloadUrl) => Quad.CreateSafe(_storage.Id!, Namespaces.FileContent.HasDownloadUrl, downloadUrl, _memberOfNamedGraph!);

    #endregion
    private static byte[] ToByteArray(Stream content)
    {
        byte[] contentAsByteArray = new byte[content.Length];
        content.Read(contentAsByteArray, 0, (int)content.Length);
        return contentAsByteArray;
    }

    public IEnumerable<Triple> Build()
    {
        //TODO create FileException
        if (_storage.Id == null) throw new RecordException("File needs ID.");
        if (_storage.Content == null) throw new RecordException("File needs content");
        if (_storage.DownLoadUrl == null) throw new RecordException("File needs a downloadURL");
        if (_storage.FileName == null) throw new RecordException("File needs a name.");
        if (_storage.MediaType == null) throw new RecordException("File needs a mediatype");

        var typeQuad = Quad.CreateSafe(_storage.Id, Namespaces.Rdf.Type, Namespaces.FileContent.Type, _memberOfNamedGraph!);
        var recordQuads = new List<Quad>
        {
            CreateMediaTypeQuad(_storage.MediaType),
            CreateByteSizeQuad(_storage.ByteSize),
            CreateFileNameQuad(_storage.FileName),
            CreateLanguageQuad(_storage.Language),
            CreateDownloadUrlQuads(_storage.DownLoadUrl),
            typeQuad
        };

        if (!string.IsNullOrEmpty(_storage.Language)) recordQuads.Add(CreateLanguageQuad(_storage.Language));
        if (!string.IsNullOrEmpty(_storage.IssuedDate)) recordQuads.Add(CreateIssuedDateQuad(_storage.IssuedDate));
        recordQuads.AddRange(CreateChecksumQuad(_storage.Checksum));

        return recordQuads.Select(quad => quad.ToTriple());
    }

}
