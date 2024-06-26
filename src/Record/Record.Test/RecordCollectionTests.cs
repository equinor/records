using FluentAssertions;
using Records.Collection;
using VDS.RDF.Parsing;

namespace Records.Tests;

public class RecordCollectionTests
{
    [Fact]
    public void RecordCollection_HasTheSameElements_AfterCreation()
    {
        var recordIds = Enumerable.Range(1, 5)
            .Select(TestData.CreateRecordId)
            .ToList();

        var records = recordIds
            .Select(id => TestData.ValidRecord(id))
            .ToList();

        var recordCollection = new RecordCollection(records);

        recordCollection.Records.SequenceEqual(records).Should().BeTrue();
        recordCollection.Records.Should().HaveCount(records.Count);
        recordCollection.Records.Should().BeEquivalentTo(records);
    }

    [Fact]
    public void ListOfRecordStrings_ShouldParse_IntoRecordCollection()
    {
        var recordIds = Enumerable.Range(1, 5)
            .Select(TestData.CreateRecordId)
            .ToList();

        var records = recordIds
            .Select(id => TestData.ValidRecord(id))
            .ToList();

        var recordStrings = records
            .Select(record => record.ToString())
            .ToList();

        var recordCollection = new RecordCollection(recordStrings, new NQuadsParser());

        recordCollection.Records.SequenceEqual(records).Should().BeTrue();
        recordCollection.Records.Should().HaveCount(records.Count);
        recordCollection.Records.Should().BeEquivalentTo(records);
    }

    [Fact]
    public void ConcatenatedRecordStrings_ShouldParse_IntoRecordCollection()
    {
        var recordIds = Enumerable.Range(1, 5)
            .Select(TestData.CreateRecordId)
            .ToList();

        var records = recordIds
            .Select(id => TestData.ValidRecord(id))
            .ToList();

        var recordStrings = string.Join("\n", records.Select(record => record.ToString()));

        var recordCollection = new RecordCollection(recordStrings, new NQuadsParser());

        recordCollection.Records.SequenceEqual(records).Should().BeTrue();
        recordCollection.Records.Should().HaveCount(records.Count);
        recordCollection.Records.Should().BeEquivalentTo(records);
    }
}
