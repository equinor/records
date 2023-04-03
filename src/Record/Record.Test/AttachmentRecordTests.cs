using System.Text.Json;
using System.Text.Json.Nodes;
using FluentAssertions;
using Record = Records.Immutable.Record;
using Records.Exceptions;
using VDS.RDF.Writing;

namespace Records.Tests;

public class AttachmentRecordTests
{
    private readonly string _id;
    private readonly string _attachmentIri;
    private readonly string _scope;
    private readonly string[] _additionalScopes;

    public AttachmentRecordTests()
    {
        _id = TestData.CreateRecordId("1");
        _attachmentIri = TestData.CreateRecordIri("attachmentIri", "1");
        _scope = TestData.CreateRecordIri("attachmentReference", "1");
        _additionalScopes = TestData.CreateObjectList(3, "scopes").ToArray();
    }

    [Fact]
    public void Add_Quad_To_Content_If_Object_Not_Null()
    {
        var mediaType = TestData.CreateRecordIri("mediaType", "1");
        var mediaQuad = Quad.CreateSafe(_attachmentIri, "http://www.w3.org/ns/dcat#mediaType", mediaType, _id);

        var record = Record.CreateAttachmentRecord(_id,
            _attachmentIri,
            _scope,
            _additionalScopes,
            mediaType: mediaType);

        record.ContainsQuad(mediaQuad).Should().BeTrue();
        record.Quads().Count().Should().Be(7);
    }

    [Fact]
    public void Attachment_Record_Should_Describe_AttachmentIri()
    {
        var record = Record.CreateAttachmentRecord(_id, _attachmentIri, _scope, _additionalScopes);

        record.Describes.Should().BeEquivalentTo(_attachmentIri);
    }

    [Fact]
    public void Checksum_Not_Null_Should_Add_Three_Blank_Nodes()
    {
        var checksumValue = TestData.CreateRecordIri("checkSumValue", "0");
        var checksumAlgorithm = TestData.CreateRecordIri("MD5", "0");

        var record = Record.CreateAttachmentRecord(_id,
            _attachmentIri,
            _scope,
            _additionalScopes,
            checksum: checksumValue,
            checksumAlgorithm: checksumAlgorithm);

        record.Quads().Select(q => q.Object).Where(o => o.StartsWith("_:")).Count().Should().Be(1);
        record.Quads().Select(q => q.Subject).Where(s => s.StartsWith("_:")).Count().Should().Be(2);
        record.Quads().Count().Should().Be(9);
    }



}
