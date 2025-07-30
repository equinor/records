using System.Text;
using FluentAssertions;
using Records.Exceptions;
using VDS.RDF;
using VDS.RDF.Writing;
using Record = Records.Immutable.Record;
namespace Records.Tests;

public class FileRecordBuilderTests
{
    [Fact]
    public void FileRecordBuilder_ShouldCreate_ValidRecordWithFileContent()
    {

        var superRecordId = TestData.CreateRecordId(Guid.NewGuid().ToString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");
        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithIsSubRecordOf(superRecordId)
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        var recstring = fileRecord.ToString<TriGWriter>();

        fileRecord.Should().NotBeNull();
        fileRecord.TriplesWithPredicate(Namespaces.Record.IsSubRecordOf).Select(q => q.Object.ToString()).Single().Should().Be(superRecordId);
        fileRecord.TriplesWithPredicate(Namespaces.FileContent.generatedAtTime).Count().Should().Be(1);
        fileRecord.TriplesWithPredicate(Namespaces.Rdf.Type).Count().Should().Be(2);
    }


    [Fact]
    public void FileRecordBuilder__ShouldCreateUriNode__WhenObjectIsFileType()
    {
        var superRecordId = TestData.CreateRecordId(Guid.NewGuid().ToString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");

        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithIsSubRecordOf(superRecordId)
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        var fileTypeNode = fileRecord.TriplesWithObject(Namespaces.FileContent.Type).Select(q => q.Object).First();
        fileTypeNode.Should().NotBeNull("The variable fileTypeNode is null which means that it is not an uri node.");

    }

    [Fact]
    public void FileRecordBuilder__ShouldCreateDervivedFrom()
    {
        var superRecordId = TestData.CreateRecordId(Guid.NewGuid().ToString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");

        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithDerivedFrom("https://example.com/derivedFrom")
            .WithIsSubRecordOf(superRecordId)
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        var derivedFromNode = fileRecord.TriplesWithPredicate(Namespaces.Prov.WasDerivedFrom).Select(q => q.Object.ToString()).First();
        derivedFromNode.Should().Be("https://example.com/derivedFrom");
    }


    [Fact]
    public void FileRecordBuilder__ShouldCreateDerivedFrom__FromUri()
    {
        var superRecordId = TestData.CreateRecordId(Guid.NewGuid().ToString());
        var fileRecordId = TestData.CreateRecordId("fileRecordId");
        var scopes = TestData.CreateObjectList(3, "scope");
        var httpsExampleComDescribes = new Uri("https://example.com/derivedFrom");

        var fileRecord = new FileRecordBuilder()
            .WithId(fileRecordId)
            .WithDerivedFrom(httpsExampleComDescribes)
            .WithIsSubRecordOf(superRecordId)
            .WithFileExtension("xslx")
            .WithFileName("filename")
            .WithScopes(scopes)
            .WithDocumentType("doctype")
            .WithModelType("modeltype")
            .WithLanguage("en-US")
            .WithFileContent(Encoding.UTF8.GetBytes("This is very cool file content B-)"))
            .Build();

        var describesNode = fileRecord.TriplesWithPredicate(Namespaces.Prov.WasDerivedFrom).Select(q => q.Object.ToString()).First();
        describesNode.Should().Be(httpsExampleComDescribes.ToString());
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

