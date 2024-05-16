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
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
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
    public void FileRecordBuilder__ShouldCreateUriNode__WhenObjectIsFileType()
    {
        var superRecord = new Record(TestData.ValidJsonLdRecordString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");
        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithIsSubRecordOf(superRecord.Id)
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        var fileTypeNode = fileRecord.QuadsWithObject(Namespaces.FileContent.Type).Select(q => q.Object).First();
        fileTypeNode.Should().NotBeNull("The variable fileTypeNode is null which means that it is not an uri node.");

    }

    [Fact]
    public void FileRecordBuilder__ShouldCreateDescribes()
    {
        var superRecord = new Record(TestData.ValidJsonLdRecordString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");
        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithDescribes("https://example.com/describes")
            .WithIsSubRecordOf(superRecord.Id)
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        var describesNode = fileRecord.QuadsWithPredicate(Namespaces.Record.Describes).Select(q => q.Object).First();
        describesNode.Should().Be("https://example.com/describes");


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
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
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
                .WithDocumentType("doctype")
                .WithFileExtension("xslx")
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
    public void FileRecordBuilder__ShouldThrowException__WhenDocumentTypeIsMissing()
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
                .WithFileExtension("xslx")
                .WithModelType("modeltype")
                .WithLanguage("en-US")
                .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
                .Build();
            };

            fileRecordBuilder.Should()
                .Throw<FileRecordException>()
                .WithMessage("Failure in building file record. File record needs the document type of the file.");

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
            .WithFileExtension("xslx")
            .WithScopes(scopes)
            .WithFileName("filename")
            .WithLanguage("en-US")
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
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
            .WithFileExtension("xslx")
             .WithDocumentType("doctype")
            .WithModelType("modeltype")
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
    public void FileRecordBuilder__ShouldThrowException__WhenFileExtensionIsMissing()
    {
        var fileRecord = default(Record);
        var scopes = TestData.CreateObjectList(3, "scope");

        var fileRecordBuilder = () =>
        {
            fileRecord = new FileRecordBuilder()
            .WithId(TestData.CreateRecordId("fileRecordId"))
            .WithIsSubRecordOf(TestData.CreateRecordId("superRecordId"))
            .WithScopes(scopes)
            .WithFileName("filename")
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();
        };

        fileRecordBuilder.Should()
            .Throw<FileRecordException>()
            .WithMessage("Failure in building file record. File record needs the file extension.");

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
            .WithFileName("fileName")
            .WithFileExtension("xslx")
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
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

