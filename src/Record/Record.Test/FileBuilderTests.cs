using System.Text;
using FluentAssertions;

namespace Records.Tests;

public class FileBuilderTests
{
    [Fact]
    public void FileRecordBuilder_ShouldCreate_ValidRecordWithFileContent()
    {

        var fileRecord = new FileRecordBuilder()
            .WithId("https://example.com/fileRecordId")
            .WithIsSubRecordOf("https://example.com/superRecordId")
            .WithScopes("ex:scope1", "ex:scope2", "ex:scope3")
            .WithMediaType("xlsx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content"))
            .Build();

        fileRecord.Should().NotBeNull();
    }
}

