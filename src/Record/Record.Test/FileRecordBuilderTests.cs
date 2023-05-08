using System.Text;
using FluentAssertions;
using Records.Exceptions;
using Record = Records.Immutable.Record;
namespace Records.Tests;

public class FileRecordBuilderTests
{
    [Fact]
    public void FileRecordBuilder_ShouldCreate_ValidRecordWithFileContent()
    {
        var superRecord = new Record(TestData.ValidJsonLdRecordString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithIsSubRecordOf(superRecord.Id)
            .WithMediaType("xslx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        fileRecord.Should().NotBeNull();
        fileRecord.QuadsWithPredicate(Namespaces.Record.IsSubRecordOf).Select(q => q.Object).Single().Should().Be(superRecord.Id);
        fileRecord.QuadsWithPredicate(Namespaces.Record.Describes).Select(q => q.Object).Single().Should().Be(fileRecordId);
        fileRecord.QuadsWithPredicate(Namespaces.FileContent.generatedAtTime).Count().Should().Be(1);
        fileRecord.QuadsWithPredicate(Namespaces.Rdf.Type).Count().Should().Be(2);
    }

    [Fact]
    public void FileRecordBuilder__SHouldThrowException__WhenSuperRecordIsNotProvided()
    {
        var fileRecord = default(Record);

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithMediaType("xslx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs to have a subrecord relation.");

        fileRecord.Should().BeNull();

    }

    [Fact]
    public void FileRecordBuilder__ShouldThrowException__WhenContentIsNotProvided()
    {
        var fileRecord = default(Record);

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithMediaType("xslx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs content.");

        fileRecord.Should().BeNull();
    }


    [Fact]
    public void FileRecordBuilder__ShouldThrowException__WhenFileNameIsNotProvided()
    {
        var fileRecord = default(Record);

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithMediaType("xslx")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs a file name.");

        fileRecord.Should().BeNull();
    }


    [Fact]
    public void FileRecordBuilder__ShouldThrowException__WhenMediaTypeIsNotProvided()
    {
        var fileRecord = default(Record);

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs the media type of the file.");

        fileRecord.Should().BeNull();
    }


}

