using FluentAssertions;
using Record = Records.Immutable.Record;

namespace Records.Tests;

public class RecordRepositoryTests
{
    [Fact]
    public void RecordRepository_Can_Add_Records()
    {
        var repo = new RecordRepository();
        var record1 = TestData.ValidRecord(TestData.CreateRecordId("1"));
        var record2 = TestData.ValidRecord(TestData.CreateRecordId("2"));

        repo.Add(record2);
        repo.Add(record1);

        var result = repo.Count;
        result.Should().Be(2);
    }

    [Fact]
    public void RecordRepository_Can_Remove_Records()
    {
        var repo = new RecordRepository();
        var record1 = TestData.ValidRecord(TestData.CreateRecordId("1"));
        var record2 = TestData.ValidRecord(TestData.CreateRecordId("2"));

        repo.Add(record2);
        repo.Add(record1);

        var firstCountResult = repo.Count;
        firstCountResult.Should().Be(2);

        repo.TryRemove(record1);
        var result = repo.Count;
        result.Should().Be(1);
    }

    [Fact]
    public void RecordRepository_Can_Be_Initialised_With_Collection()
    {
        var record1 = TestData.ValidRecord(TestData.CreateRecordId("1"));
        var record2 = TestData.ValidRecord(TestData.CreateRecordId("2"));

        var repo = new RecordRepository(new[] { record1, record2 });
        var repoCount = repo.Count;

        repoCount.Should().Be(2);
    }

    [Fact]
    public void RecordRepository_Can_Validate()
    {
        var record1 = TestData.ValidRecord(TestData.CreateRecordId("1"), 3, 1);
        var record2 = TestData.ValidRecord(TestData.CreateRecordId("2"), 2, 1);
        
        var repo = new RecordRepository(new[] { record1, record2 });
        var result = repo.Validate();

        result.Valid.Should().Be(true);
    }

    [Fact]
    public void RecordRepository_Fails_Validation()
    {
        var record1 = TestData.ValidRecord(TestData.CreateRecordId("1"));
        var record2 = TestData.ValidRecord(TestData.CreateRecordId("2"));

        var repo = new RecordRepository(new[] { record1, record2 });
        var result = repo.Validate();

        result.Valid.Should().Be(false);
    }

    [Fact]
    public void RecordRepository_Can_Retrieve_Record()
    {
        var record = TestData.ValidRecord();

        var repo = new RecordRepository(record);

        var success = repo.TryGetRecord(record.Id, out var result);
        success.Should().BeTrue();
        result.Should().Be(record);
    }

    [Fact]
    public void RecordRepository_Cannot_Find_Unknown_Record()
    {
        var record = TestData.ValidRecord();
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
            var record = TestData.ValidRecord(TestData.CreateRecordId(i), 5, 5);
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
            var record = TestData.ValidRecord(TestData.CreateRecordId(i), numberOfAttributes, numberOfAttributes);
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
            var record = TestData.ValidRecord(TestData.CreateRecordId(i), 5, 5);
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
