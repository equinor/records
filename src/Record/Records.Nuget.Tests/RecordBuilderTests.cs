using FluentAssertions;
using Records.Exceptions;
using Record = Records.Immutable.Record;
using VDS.RDF;
using Newtonsoft.Json.Linq;
using VDS.RDF.Writing;
using Xunit;

namespace Records.Nuget.Tests;

public class RecordBuilderTests
{

    [Fact]
    public void Can__Create__Record()
    {
        var id = CreateRecordId("0");
        var scopes = CreateObjectList(2, "scope");
        var describes = CreateObjectList(2, "describes");
        var used = CreateObjectList(2, "used");

        var record = new RecordBuilder()
            .WithScopes(scopes)
            .WithDescribes(describes)
            .WithId(id)
            .Build();

        record.Should().NotBeNull();

        record.Scopes.Should().Contain(scopes.First());
        record.Scopes.Should().Contain(scopes.Last());

        record.Describes.Should().Contain(describes.First());
        record.Describes.Should().Contain(describes.Last());

        record.Id.Should().Be(id);
    }

    public static string CreateRecordId(string id) => $"https://ssi.example.com/record/{id}";
    public static List<string> CreateObjectList(int numberOfObjects, string subset)
        => Enumerable.Range(1, numberOfObjects)
            .Select(i => CreateRecordIri(subset, i.ToString()))
            .ToList();
    public static string CreateRecordIri(string subset, string id) => $"https://ssi.example.com/{subset}/{id}";
}