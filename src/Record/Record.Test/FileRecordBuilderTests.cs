using System.Text;
using FluentAssertions;
using Records.Exceptions;
using VDS.RDF;
using Record = Records.Immutable.Record;
namespace Records.Tests;

public class FileRecordBuilderTests
{
    [Fact]
    public void FileRecordBuilder_ShouldCreate_ValidRecordWithFileContent()
    {
        var superRecord = new Record(TestData.ValidJsonLdRecordString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");
        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithIsSubRecordOf(superRecord.Id)
            .WithFileType("xslx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithScopes(scopes)
            .WithDocumentType("LA")
            .WithModelType("MEL")
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
    public void FileRecordBuilder__SHouldThrowException__WhenSuperRecordIsMissing()
    {
        var fileRecord = default(Record);
        var scopes = TestData.CreateObjectList(3, "scope");
        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithFileType("xslx")
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithLanguage("en-US")
            .WithScopes(scopes)
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs to have a subrecord relation.");

        fileRecord.Should().BeNull();
    }


    [Fact]
    public void FileRecordBuilder__ShouldThrowException__WhenModelTypeIsMissing()
    {
        {
            var fileRecord = default(Record);
            var scopes = TestData.CreateObjectList(3, "scope");


            var fileRecordBuilder = () =>
            {
                fileRecord = new FileRecordBuilder()
                .WithId(TestData.CreateRecordId("fileRecordId"))
                .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
                .WithScopes(scopes)
                .WithFileName("fileName")
                .WithFileType("xslx")
                .WithLanguage("en-US")
                .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
                .Build();
            };

            fileRecordBuilder.Should()
                .Throw<FileRecordException>()
                .WithMessage("Failure in building file record. File record needs model type.");

            fileRecord.Should().BeNull();
        }
    }


    [Fact]
    public void FileRecordBuilder__ShouldThrowException__WhenContentIsMissing()
    {
        var fileRecord = default(Record);
        var scopes = TestData.CreateObjectList(3, "scope");

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithFileType("xslx")
            .WithScopes(scopes)
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
    public void FileRecordBuilder__ShouldThrowException__WhenFileNameIsMissing()
    {
        var fileRecord = default(Record);
        var scopes = TestData.CreateObjectList(3, "scope");

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithScopes(scopes)
            .WithFileType("xslx")
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
    public void FileRecordBuilder__ShouldThrowException__WhenMediaTypeIsMissing()
    {
        var fileRecord = default(Record);
        var scopes = TestData.CreateObjectList(3, "scope");

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithScopes(scopes)
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

    [Fact]
    public void FileRecordBuilder__ShouldThrowException__WhenScopesIsMissing()
    {
        var fileRecord = default(Record);

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithFileName("B123-EX-W-LA-XLSX")
            .WithFileType("xslx")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs scopes.");

        fileRecord.Should().BeNull();
    }

    [Fact]
    public void FileRecordBuilder__ShouldThrowAggregateException__WhenSeveralValuesIsMissing()
    {
        var fileRecord = default(Record);

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<AggregateException>();

        fileRecord.Should().BeNull();
    }


}

