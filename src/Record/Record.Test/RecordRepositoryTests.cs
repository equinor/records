using FluentAssertions;
using Record = Records.Immutable.Record;

namespace Records.Tests;

public class RecordRepositoryTests
{
    private const string rdf = @"
{
    ""@context"": {
        ""@version"": 1.1,
        ""@vocab"": ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""@base"":  ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""record"": ""https://rdf.equinor.com/ontology/record/"",
        ""eqn"": ""https://rdf.equinor.com/fam/"",
        ""akso"": ""https://akersolutions.com/data/"",
        ""record:isInScope"": { ""@type"": ""@id"" },
        ""record:describes"": { ""@type"": ""@id"" } 
    },
    ""@id"": ""akso:RecordID123"",
    ""@graph"": [
        {
            ""@id"": ""akso:RecordID123"",
            ""@type"": ""record:Record"",
            ""record:replaces"": ""https://ssi.example.com/record/0"",
            ""record:isInScope"": [
                ""eqn:TestScope1"",
                ""eqn:TestScope2""
            ],
            ""record:describes"": [
                ""eqn:Document/Wist/C277-AS-W-LA-00001.F01""
            ]
        },
        {
            ""@id"": ""eqn:Document/Wist/C277-AS-W-LA-00001.F01"",
            ""@type"": ""eqn:Revision"",
            ""RevisionSequence"": ""01"",
            ""Revision"": ""F01"",
            ""ReasonForIssue"": ""Revision text"",
            ""Author"": ""Kari Nordkvinne"",
            ""CheckedBy"": ""NN"",
            ""DisciplineApprovedBy"": ""NM""
        }
    ]
}
        ";

    private const string rdf2 = @"
{
    ""@context"": {
        ""@version"": 1.1,
        ""@vocab"": ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""@base"":  ""https://rdf.equinor.com/ontology/fam/v1/"",
        ""record"": ""https://rdf.equinor.com/ontology/record/"",
        ""eqn"": ""https://rdf.equinor.com/fam/"",
        ""akso"": ""https://akersolutions.com/data/""
    },
    ""@id"": ""akso:RecordID123"",
    ""@graph"": [
        {
            ""@id"": ""akso:RecordID123"",
            ""record:replaces"": ""https://ssi.example.com/record/0"",
            ""record:isInScope"": [
                ""eqn:TestScope1"",
                ""eqn:TestScope2""
            ],
            ""record:describes"": [
                ""eqn:Document/Wist/C277-AS-W-LA-00001.F01""
            ]
        },
        {
            ""@id"": ""eqn:Document/WIST/C277-AS-W-LA-00001.F01"",
            ""@type"": ""eqn:Revision"",
            ""RevisionSequence"": ""01"",
            ""Revision"": ""F01"",
            ""ReasonForIssue"": ""Revision text"",
            ""Author"": ""Kari Nordkvinne"",
            ""CheckedBy"": ""NN"",
            ""DisciplineApprovedBy"": ""NM""
        }
    ]
}
        ";

    private const string rdf3 = @"<http://example.com/data/Object1/Record0> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/record/Record> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/describes> <http://example.com/data/Object1> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/isInScope> <http://example.com/data/Project> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1/Record0> <https://rdf.equinor.com/ontology/record/replaces> <http://ssi.example.com/record/0> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <https://rdf.equinor.com/ontology/mel/System> <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Length> ""0"" <http://example.com/data/Object1/Record0> .
<http://example.com/data/Object1> <http://rds.posccaesar.org/ontology/plm/rdl/Weight> ""0"" <http://example.com/data/Object1/Record0> .";

    [Fact]
    public void RecordRepository_Can_Add_Records()
    {
        var repo = new RecordRepository();
        var record1 = new Record(rdf);
        var record2 = new Record(rdf3);

        repo.Add(record2);
        repo.Add(record1);

        var result = repo.Count;
        result.Should().Be(2);
    }

    [Fact]
    public void RecordRepository_Can_Remove_Records()
    {
        var repo = new RecordRepository();
        var record1 = new Record(rdf);
        var record2 = new Record(rdf3);

        repo.Add(record2);
        repo.Add(record1);

        var firstCountResult = repo.Count;
        firstCountResult.Should().Be(2);

        repo.TryRemove(record1);
        var result = repo.Count;
        result.Should().Be(1);
    }

    [Fact]
    public void RecordRepository_Is_Serialisable()
    {
        var repo = new RecordRepository();
        var record1 = new Record(rdf);
        var record2 = new Record(rdf3);

        repo.Add(record2);
        repo.Add(record1);

        var result = repo.ToString().Split("\n").Length;
        result.Should().Be(20);
    }

    [Fact]
    public void RecordRepository_Can_Be_Initialised_With_Collection()
    {
        var record1 = new Record(rdf);
        var record2 = new Record(rdf3);

        var repo = new RecordRepository(new[] { record1, record2 });
        var repoCount = repo.Count;

        repoCount.Should().Be(2);
    }

    [Fact]
    public void RecordRepository_Can_Validate()
    {
        var record1 = new Record(RecordTests.RandomRecord(id: "0", numberDescribes: 3, numberScopes: 1));
        var record2 = new Record(RecordTests.RandomRecord(id: "1", numberDescribes: 2, numberScopes: 1));

        var repo = new RecordRepository(new[] { record1, record2 });
        var result = repo.Validate();

        result.Valid.Should().Be(true);
    }

    [Fact]
    public void RecordRepository_Fails_Validation()
    {
        var record1 = new Record(RecordTests.RandomRecord(id: "1", numberDescribes: 2, numberScopes: 1));
        var record2 = new Record(RecordTests.RandomRecord(id: "2", numberDescribes: 2, numberScopes: 1));

        var repo = new RecordRepository(new[] { record1, record2 });
        var result = repo.Validate();

        result.Valid.Should().Be(false);
    }

    [Fact]
    public void RecordRepository_Can_Retrieve_Record()
    {
        var record = new Record(RecordTests.RandomRecord("0", 2, 1));
        var repo = new RecordRepository(record);

        var success = repo.TryGetRecord(record.Id, out var result);
        success.Should().BeTrue();
        result.Should().Be(record);
    }

    [Fact]
    public void RecordRepository_Cannot_Find_Unknown_Record()
    {
        var record = new Record(RecordTests.RandomRecord("0", 2, 1));
        var repo = new RecordRepository(record);

        var success = repo.TryGetRecord("https://ssi.example.com/invalid/id", out var result);
        success.Should().BeFalse();
        result.Should().BeNull();
    }

    [Fact]
    public void RecordRepository_Can_Enumerate()
    {
        var numberOfRecord = 10;
        var repo = new RecordRepository();

        for (var i = 0; i < numberOfRecord; i++)
        {
            var record = new Record(RecordTests.RandomRecord(i.ToString(), 5, 5));
            repo.Add(record);
        }

        var count = 0;
        foreach (var record in repo)
        {
            record.Should().BeOfType<Record>();
            record.Should().NotBeNull();
            count += 1;
        }

        count.Should().Be(numberOfRecord);
    }

    [Fact]
    public void RecordRepository_Has_Linq()
    {
        var totalRecords = 20;
        var halfRecords = 5;
        var fullRecords = 10;

        var totalScopes = ((totalRecords / 2) * halfRecords) + ((totalRecords / 2) * fullRecords);

        var repo = new RecordRepository();

        for (var i = 0; i < totalRecords; i++)
        {
            var numberOfAttributes = (i < (totalRecords / 2)) ? halfRecords : fullRecords;
            var record = new Record(RecordTests.RandomRecord(i.ToString(), numberOfAttributes, numberOfAttributes));
            repo.Add(record);
        }

        var numTen = repo.Where(r => r.Scopes.Count == 5).Select(r => r).Count();
        numTen.Should().Be(10);

        var numTotalScopes = repo.SelectMany(r => r.Scopes).Count();
        numTotalScopes.Should().Be(totalScopes);
    }

    [Fact]
    public void RecordRepository_Contains()
    {
        const int numberOfRecord = 10;
        var repo = new RecordRepository();

        for (var i = 0; i < numberOfRecord; i++)
        {
            var record = new Record(RecordTests.RandomRecord(i.ToString(), 5, 5));
            repo.Add(record);
        }

        for (var i = 0; i < numberOfRecord; i++)
        {
            var resultTrue = repo.Contains($"https://ssi.example.com/record/{i}");
            resultTrue.Should().BeTrue();
        }

        var resultFalse = repo.Contains($"https://ssi.example.com/record/{numberOfRecord + 1}");
        resultFalse.Should().BeFalse();
    }
}
