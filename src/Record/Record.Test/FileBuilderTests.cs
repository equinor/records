using System.Text;
using FluentAssertions;
using Record = Records.Immutable.Record;
namespace Records.Tests;

public class FileBuilderTests
{
    [Fact]
    public void FileRecordBuilder_ShouldCreate_ValidRecordWithFileContent()
    {
        var superRecord = new Record(TestData.ValidJsonLdRecordString());


        var fileRecord = new FileRecordBuilder()
            .WithId("https://example.com/fileRecordId")
            .WithIsSubRecordOf(superRecord.Id)
            .WithMediaType("xlsx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        fileRecord.Should().NotBeNull();
    }
}

