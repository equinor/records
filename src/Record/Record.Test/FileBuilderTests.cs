using System.Text;
using FluentAssertions;
using Record = Records.Immutable.Record;

namespace Records.Tests;

public  class FileBuilderTests
{
    [Fact]
    public void File_Should_Not_Be_Null()
    {
        var file = new FileBuilder()
            .WithId("att:B123-EX-W-LA-XLSX")
            .WithNamedGraph("ex:recordId")
            .WithMediaType("xlsx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithIssuedDate("04.04.2023")
            .WithByteSize(1.2)
            .WithLanguage("en-US")
            .WithCheckSum(Encoding.UTF8.GetBytes("This is file content"))
            .WithDownloadUrl("ex:downloadme.no")
            .Build();

        var attachmentRecord = new RecordBuilder()
            .WithId("ex:recordId")
            .WithContent(file)
            .WithDescribes("att:B123-EX-W-LA-XLSX")
            .WithScopes("ex:attachmentRecord")
            .Build();

        file.Should().NotBeNull();
        attachmentRecord.Should().NotBeNull();
    }

}
