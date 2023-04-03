
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
    string attachmentIri,
    string scope,
    string[] additionalScopes,
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
        CreateChecksumQuad(quadList, attachmentIri, recordId, checksum, checksumAlgorithm);
        CreateMediaTypeQuad(quadList, attachmentIri, recordId, mediaType);
        CreateByteSizeQuad(quadList, attachmentIri, recordId, byteSize);
        CreateFileNameQuad(quadList, attachmentIri, recordId, fileName);
        CreateIssuedDateQuad(quadList, attachmentIri, recordId, issuedDate.ToString());
        CreateLanguageQuad(quadList, attachmentIri, recordId, language);
        CreateDownloadUrlQuads(quadList, attachmentIri, recordId, downloadUrls);

        var record = new RecordBuilder()
            .WithId(recordId)
            .WithScopes(scope)
            .WithAdditionalScopes(additionalScopes)
            .WithDescribes(attachmentIri)
            .WithContent(quadList)
            .Build();

        return record;
    }

    private static void CreateChecksumQuad(List<Quad> quadList, string attachmentIri, string recordId, string? checksum, string? checksumAlgorithm)
    {
        if (checksum != null && checksumAlgorithm != null)
        {
            quadList.Add(Quad.CreateSafe(attachmentIri, "http://spdx.org/rdf/terms#Checksum", "_:checksum", recordId));
            quadList.Add(Quad.CreateSafe("_:checksum", "http://spdx.org/rdf/terms#algorithm", checksumAlgorithm, recordId));
            quadList.Add(Quad.CreateSafe("_:checksum", "http://spdx.org/rdf/terms#checksumValue", checksum, recordId));
        }
    }

    private static void CreateMediaTypeQuad(List<Quad> quadList, string attachmentIri, string recordId, string? mediaType) => CreateQuadIfObjectNotNull(quadList, attachmentIri, "http://www.w3.org/ns/dcat#mediaType", mediaType, recordId);

    private static void CreateByteSizeQuad(List<Quad> quadList, string attachmentIri, string recordId, string? byteSize) => CreateQuadIfObjectNotNull(quadList, attachmentIri, "http://www.w3.org/ns/dcat#byteSize", byteSize, recordId);

    private static void CreateFileNameQuad(List<Quad> quadList, string attachmentIri, string recordId, string? fileName) => CreateQuadIfObjectNotNull(quadList, attachmentIri, "http://purl.org/dc/terms/title", fileName, recordId);

    private static void CreateIssuedDateQuad(List<Quad> quadList, string attachmentIri, string recordId, string? issuedDate) => CreateQuadIfObjectNotNull(quadList, attachmentIri, "http://purl.org/dc/terms/issued", issuedDate, recordId);

    private static void CreateLanguageQuad(List<Quad> quadList, string attachmentIri, string recordId, string? language) => CreateQuadIfObjectNotNull(quadList, attachmentIri, "http://purl.org/dc/terms/language", language, recordId);

    private static void CreateDownloadUrlQuads(List<Quad> quadList, string attachmentIri, string recordId, string[]? downloadUrl) => downloadUrl?.ToList().ForEach(url => CreateQuadIfObjectNotNull(quadList, attachmentIri, "http://www.w3.org/ns/dcat#downloadURL", url, recordId));

    private static void CreateQuadIfObjectNotNull(List<Quad> quadList, string attachmentIri, string predicate, string? @object, string recordId)
    {
        if (!string.IsNullOrEmpty(@object)) quadList.Add(Quad.CreateSafe(attachmentIri, predicate, @object, recordId));
    }
}
