using FluentAssertions;
using Records.Exceptions;

namespace Records.Tests;
public class MutableRecordTests
{
    [Fact]
    public void MutableRecord_Can_Initialise_From_ImmutableRecord()
    {
        var original = TestData.CreateRecordId("original");
        var immutable = TestData.ValidRecord();

        var record = new Mutable.Record(immutable)
            .WithAdditionalReplaces(original);

        record.QuadStrings
            .Should()
            .Contain($"<{immutable.Id}> <{Namespaces.Record.Replaces}> <{original}> <{immutable.Id}> .");

        record.Id.Should().Be(immutable.Id);
    }

    [Fact]
    public void MutableRecord_Can_Be_Set_Immutable()
    {
        var id = TestData.CreateRecordId("1");
        var scope = TestData.CreateObjectList(5, "scope").ToArray();
        var describes = TestData.CreateObjectList(5, "describes").ToArray();

        var mutable = new Mutable.Record(id)
            .WithAdditionalScopes(scope)
            .WithAdditionalDescribes(describes)
            .ToImmutable();

        mutable.Should().NotBeNull();
    }

    [Fact]
    public void MutableRecord_Can_Receive_TripleStrings()
    {
        var id = TestData.CreateRecordId("mutable");

        var (s, p, o) = TestData.CreateRecordTripleStringTuple("mute");
        var scope = TestData.CreateRecordIri("scope", "1");
        var describes = TestData.CreateRecordIri("describes", "1");

        var mutableRecord = new Mutable.Record(id)
            .WithAdditionalTriples($"<{s}> <{p}> <{o}> .")
            .WithAdditionalTriples($"<{s}> <{p}> \"hello there\" .")
            .WithAdditionalTriples($"_:blank <{p}> \"hello there\" .")
            .WithAdditionalScopes(scope)
            .WithAdditionalDescribes(describes);

        var (s2, p2, o2) = TestData.CreateRecordTripleStringTuple("extra");
        mutableRecord.AddTriples($"<{s2}> <{p2}> <{o2}> .");
        mutableRecord.Id.Should().Be(id);
        mutableRecord.QuadStrings.Should().Contain($"<{s}> <{p}> <{o}> <{id}> .");
        mutableRecord.QuadStrings.Should().Contain($"<{s2}> <{p2}> <{o2}> <{id}> .");

        var immutable = mutableRecord.ToImmutable();

        immutable.Should().NotBeNull();
        immutable.Id.Should().Be(id);
    }

    [Fact]
    public void MutableRecord_Can_Receive_SafeQuads()
    {
        var id = TestData.CreateRecordId("mute");
        var mutable = new Mutable.Record(id);

        var quadNum = 10;
        for (var i = 0; i < quadNum; i++)
        {
            var (s, p, o) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var quad = Quad.CreateSafe(s, p, o, id);
            mutable.AddQuads(quad);
        }

        for (var i = 0; i < quadNum; i++)
        {
            var (s, p, o) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var quad = Quad.CreateSafe(s, p, o, id);
            mutable.QuadStrings.Should().Contain(quad.ToString());
        }

        mutable.AddScopes("https://example.com/scope/0");
        mutable.AddDescribes("https://example.com/desc/wow");

        var record = mutable.ToImmutable();
        record.Should().NotBeNull();
        record.Id.Should().Be(id);
    }

    [Fact]
    public void MutableRecord_Can_Add_IsSubRecordOf()
    {
        var superRecord = TestData.CreateRecordId("super");
        var immutable = TestData.ValidRecord();

        var record = default(Immutable.Record);

        var process = () => record = new Mutable.Record(immutable)
            .WithIsSubRecordof(superRecord)
            .ToImmutable();

        process.Should().NotThrow();
        record.Should().NotBeNull();

        record.IsSubRecordOf.Should().Be(superRecord);
    }

    [Fact]
    public void MutableRecord_Does_Not_Check_Multiple_SubRecordOf()
    {
        var immutable = TestData.ValidRecordBeforeBuildComplete()
            .WithIsSubRecordOf(TestData.CreateRecordId("super"))
            .Build();

        var record = default(Mutable.Record);

        var superRecord = TestData.CreateRecordId("superduper");
        var mutableBuild = () => record = new Mutable.Record(immutable)
            .WithIsSubRecordof(superRecord);

        mutableBuild.Should().NotThrow();
        record.Should().NotBeNull();

        var immutableBuild = () => record.ToImmutable();
        immutableBuild.Should()
            .Throw<RecordException>()
            .WithMessage("Failure in record. A record can at most be the subrecord of one other record.");


    }
}

