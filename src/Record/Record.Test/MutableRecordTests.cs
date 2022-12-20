using FluentAssertions;
using Records.Exceptions;

namespace Records.Tests;
public class MutableRecordTests
{
    [Fact]
    public void MutableRecord_Can_Initialise_From_ImmutableRecord()
    {
        var original = "https://ssi.example.com/record/original";

        var immutable = new Immutable.Record(ImmutableRecordTests.rdf);
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
        var id = QuadTests.CreateRecordId("1");
        var scope = QuadTests.CreateRecordIri("scope", "0");
        var describes = QuadTests.CreateRecordIri("describes", "0");

        var mutable = new Mutable.Record(id)
            .WithAdditionalScopes(scope)
            .WithAdditionalDescribes(describes)
            .ToImmutable();

        mutable.Should().NotBeNull();
    }

    [Fact]
    public void MutableRecord_Can_Receive_TripleStrings()
    {
        var id = QuadTests.CreateRecordId("mutable");

        var (s, p, o) = QuadTests.CreateRecordTriple("mute");
        var scope = QuadTests.CreateRecordIri("scope", "1");
        var describes = QuadTests.CreateRecordIri("describes", "1");

        var mutableRecord = new Mutable.Record(id)
            .WithAdditionalTriples($"<{s}> <{p}> <{o}> .")
            .WithAdditionalTriples($"<{s}> <{p}> \"hello there\" .")
            .WithAdditionalTriples($"_:blank <{p}> \"hello there\" .")
            .WithAdditionalScopes(scope)
            .WithAdditionalDescribes(describes);

        var (s2, p2, o2) = QuadTests.CreateRecordTriple("extra");
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
        var id = QuadTests.CreateRecordId("mute");
        var mutable = new Mutable.Record(id);

        var quadNum = 10;
        for (var i = 0; i < quadNum; i++)
        {
            var (s, p, o) = QuadTests.CreateRecordTriple(i.ToString());
            var quad = Quad.CreateSafe(s, p, o, id);
            mutable.AddQuads(quad);
        }

        for (var i = 0; i < quadNum; i++)
        {
            var (s, p, o) = QuadTests.CreateRecordTriple(i.ToString());
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
        var superRecord = QuadTests.CreateRecordId("super");
        var immutable = new Immutable.Record(ImmutableRecordTests.rdf3);

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
        var superRecord = QuadTests.CreateRecordId("super");
        var immutable = new Immutable.Record(ImmutableRecordTests.rdf4);

        var record = default(Mutable.Record);

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

