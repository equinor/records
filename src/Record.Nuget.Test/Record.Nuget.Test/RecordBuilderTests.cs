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

    [Fact]
    public void Can__Find__Version()
    {
        var id = CreateRecordId("0");
        var scopes = CreateObjectList(2, "scope");
        var describes = CreateObjectList(2, "describes");

        var record = new RecordBuilder()
            .WithScopes(scopes)
            .WithDescribes(describes)
            .WithId(id)
            .Build();

        var query = new SparqlQueryParser().ParseFromString(
            $"SELECT * WHERE {{ graph <{record.Id}>  {{ <{record.Id}> <http://www.w3.org/ns/prov#wasGeneratedBy>/<http://www.w3.org/ns/prov#wasAssociatedWith> ?version . }} }}");

        var tripleStore = record.TripleStore();
        var ds = new InMemoryDataset((TripleStore)tripleStore);
        var qProcessor = new LeviathanQueryProcessor(ds);
        var qResults = qProcessor.ProcessQuery(query);

        if (qResults is SparqlResultSet qResultSet)
        {
            var version = qResultSet.Results.Single(res => res["version"]
                .ToString()
                .Equals("https://github.com/equinor/records/commit/unknown", StringComparison.OrdinalIgnoreCase));
            version.Should().NotBeNull();
        }
        else
        {
            throw new Exception("qResults is not a SparqlResultSet");
        }
    }

    private static string CreateRecordId(string id) => $"https://ssi.example.com/record/{id}";

    private static List<string> CreateObjectList(int numberOfObjects, string subset)
        => Enumerable.Range(1, numberOfObjects)
            .Select(i => CreateRecordIri(subset, i.ToString()))
            .ToList();

    private static string CreateRecordIri(string subset, string id) => $"https://ssi.example.com/{subset}/{id}";
}