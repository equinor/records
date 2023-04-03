
using System;
using System.Runtime.CompilerServices;

namespace Records.Immutable;

public partial class Record
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="recordId"><see cref=""/></param>
    /// <param name="mediaType"><see cref="http://www.w3.org/ns/dcat#mediaType"/></param>
    /// <param name="checksum"><see cref="http://spdx.org/rdf/terms#d4e1930"/></param>
    /// <param name="byteSize"><see cref="http://www.w3.org/ns/dcat#byteSize"/></param>
    /// <param name="downloadUrls"><see cref="http://www.w3.org/ns/dcat#downloadURL"/></param>
    /// 
    /// <param name="fileName"><see cref="http://purl.org/dc/terms/title"/></param>
    /// <param name="issuedDate"><see cref="http://purl.org/dc/terms/issued"/></param>
    /// <param name="language"><see cref="http://purl.org/dc/terms/language"/></param>
    /// <returns></returns>
    public static Record CreateAttachmentRecord(string recordId,
    string scope,
    string[] additionalScopes,
    string describes,
    string? mediaType = null,
    string? checksum = null,
    string? checksumAlgorithm = null,
    string? byteSize = null,
    string[]? downloadUrls = null,
    string? fileName = null,
    DateTime? issuedDate = null,
    string? language = null
    )
    {

        var quadList = new List<Quad>();
        CreateChecksumQuad(quadList, recordId, checksum, checksumAlgorithm);
        CreateMediaTypeQuad(quadList, recordId, mediaType);
        CreateByteSizeQuad(quadList, recordId, byteSize);
        CreateFileNameQuad(quadList, recordId, fileName);
        CreateIssuedDateQuad(quadList, recordId, issuedDate.ToString());
        CreateLanguageQuad(quadList, recordId, language);
        CreateDownloadUrlQuads(quadList, recordId, downloadUrls);

        var record = new RecordBuilder()
            .WithId(recordId)
            .WithScopes(scope)
            .WithAdditionalScopes(additionalScopes)
            .WithDescribes(describes)
            .WithContent(quadList)
            .Build();

        return record;
    }

    private static void CreateChecksumQuad(List<Quad> quadList, string recordId, string? checksum, string? checksumAlgorithm)
    {

        CreateQuadIfObjectNotNull(quadList, recordId, "http://spdx.org/rdf/terms#Checksum", "_:checksum");
        CreateQuadIfObjectNotNull(quadList, "_:checksum", "http://spdx.org/rdf/terms#algorithm", checksumAlgorithm);
        CreateQuadIfObjectNotNull(quadList, "_:checksum", "http://spdx.org/rdf/terms#checksumValue", checksum);
    }

    private static void CreateMediaTypeQuad(List<Quad> quadList, string recordId, string? mediaType) => CreateQuadIfObjectNotNull(quadList, recordId, "http://www.w3.org/ns/dcat#mediaType", mediaType);

    private static void CreateByteSizeQuad(List<Quad> quadList, string recordId, string? byteSize) => CreateQuadIfObjectNotNull(quadList, recordId, "http://www.w3.org/ns/dcat#byteSize", byteSize);

    private static void CreateFileNameQuad(List<Quad> quadList, string recordId, string? fileName) => CreateQuadIfObjectNotNull(quadList, recordId, "http://purl.org/dc/terms/title", fileName);

    private static void CreateIssuedDateQuad(List<Quad> quadList, string recordId, string? issuedDate) => CreateQuadIfObjectNotNull(quadList, recordId, "http://purl.org/dc/terms/issued", issuedDate);

    private static void CreateLanguageQuad(List<Quad> quadList, string recordId, string? language) => CreateQuadIfObjectNotNull(quadList, recordId, "http://purl.org/dc/terms/language", language);

    private static void CreateDownloadUrlQuads(List<Quad> quadList, string recordId, string[]? downloadUrl) => downloadUrl?.ToList().ForEach(url => CreateQuadIfObjectNotNull(quadList, recordId, "http://www.w3.org/ns/dcat#downloadURL", url));

    private static void CreateQuadIfObjectNotNull(List<Quad> quadList, string recordId, string predicate, string? @object)
    {
        if (@object != null) quadList.Add(Quad.CreateSafe(recordId, predicate, @object, recordId));
    }
}
