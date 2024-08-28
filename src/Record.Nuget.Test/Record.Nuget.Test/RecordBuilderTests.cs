using FluentAssertions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;

namespace Records.Nuget.Test;

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

    private static string CreateRecordId(string id) => $"https://ssi.example.com/record/{id}";

    private static List<string> CreateObjectList(int numberOfObjects, string subset)
        => Enumerable.Range(1, numberOfObjects)
            .Select(i => CreateRecordIri(subset, i.ToString()))
            .ToList();

    private static string CreateRecordIri(string subset, string id) => $"https://ssi.example.com/{subset}/{id}";
}