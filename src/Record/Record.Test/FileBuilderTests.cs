using System.Text;
using FluentAssertions;
using Record = Records.Immutable.Record;

namespace Records.Tests;

public class FileBuilderTests
{
    [Fact]
    public void FileBuilder_ShouldCreate_ValidRecordContent()
    {
        var attachmentRecordId = "https://example.com/recordId";
        var fileName = "B123-EX-W-LA-XLSX";

        var file = new FileBuilder()
            .WithId(attachmentRecordId)
            .WithMediaType("xlsx")
            .WithFileName(fileName)
            .WithLanguage("en-US")
            .WithContent(Encoding.UTF8.GetBytes("This is very cool file content"))
            .Build();

        var attachmentRecord = new RecordBuilder()
            .WithId(attachmentRecordId)
            .WithContent(file)
            .WithDescribes(attachmentRecordId)
            .WithScopes("ex:scope")
            .Build();

        file.Should().NotBeNull();
        attachmentRecord.Should().NotBeNull();
        attachmentRecord.QuadsWithPredicate(Namespaces.FileContent.HasTitle).Select(q => q.Object).FirstOrDefault().Should().Be(fileName);
    }
}

