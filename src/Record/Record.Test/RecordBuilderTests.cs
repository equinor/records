using FluentAssertions;
using Records.Exceptions;
using Record = Records.Immutable.Record;
using VDS.RDF;

namespace Records.Tests;
public class RecordBuilderTests
{
    [Fact]
    public void Can_Add_Scopes()
    {
        var id = TestData.CreateRecordId("0");
        var scopes = TestData.CreateObjectList(2, "scope");
        var describes = TestData.CreateObjectList(2, "describes");

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
    public void RecordBuilder_With()
    {
        var id0 = TestData.CreateRecordId("0");

        var scope0 = TestData.CreateRecordIri("scope", "0");
        var scope1 = TestData.CreateRecordIri("scope", "1");

        var builder1 = new RecordBuilder()
            .WithId(id0)
            .WithScopes(scope0);

        var builder2 = builder1.WithScopes(scope1);

        var record1 = builder1.Build();
        var record2 = builder2.Build();

        record1.Scopes.Should().NotContain(scope1);
        record2.Scopes.Should().Contain(scope1);
        record1.Id.Should().Be(record2.Id);
    }

    [Fact]
    public void RecordBuilder_Fluent()
    {
        var id0 = TestData.CreateRecordId("0");
        var id1 = TestData.CreateRecordId("1");

        var scope = TestData.CreateRecordIri("scope", "0");
        var desc = TestData.CreateRecordIri("describes", "0");

        var quads = new List<SafeQuad>();
        var numberOfQuads = 10;
        for (var i = 0; i < numberOfQuads; i++)
        {
            var (subject, predicate, @object) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var quad = Quad.CreateSafe(subject, predicate, @object, id1);
            quads.Add(quad);
        }

        var record = new RecordBuilder()
            .WithId(id1)
            .WithScopes(scope)
            .WithDescribes(desc)
            .WithAdditionalContent(quads)
            .WithReplaces(id0)
            .Build();

        record.Id.Should().Be(id1);
        record.Scopes.Should().Contain(scope);
        record.Describes.Should().Contain(desc);
        record.Replaces.Should().Contain(id0);
        record.Quads().ToList().Should().Contain(quads);
    }

    [Fact]
    public void RecordBuilder_Fails_With_No_Scopes()
    {
        var id0 = TestData.CreateRecordId("0");
        var id1 = TestData.CreateRecordId("1");

        var scope = TestData.CreateRecordIri("scope", "0");
        var desc = TestData.CreateRecordIri("describes", "0");

        var quads = new List<SafeQuad>();
        var numberOfQuads = 10;
        for (var i = 0; i < numberOfQuads; i++)
        {
            var (subject, predicate, @object) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var quad = Quad.CreateSafe(subject, predicate, @object, id1);
            quads.Add(quad);
        }

        var builder = new RecordBuilder()
            .WithId(id1)
            .WithDescribes(desc)
            .WithAdditionalContent(quads)
            .WithReplaces(id0);

        var result = () => builder.Build();

        result.Should().ThrowExactly<RecordException>().WithMessage("Failure in record. A record must either be a subrecord or have at least one scope");
    }

    [Fact]
    public void RecordBuilder_Can_Add_Triples()
    {
        var graph = new Graph();

        var id0 = TestData.CreateRecordId("0");
        graph.BaseUri = new Uri(id0);

        var scope = TestData.CreateRecordIri("scope", "0");
        var desc = TestData.CreateRecordIri("describes", "0");

        var numberOfTriples = 10;
        for (var i = 0; i < numberOfTriples; i++)
        {
            var (subject, predicate, @object) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var sub = graph.CreateUriNode(new Uri(subject));
            var pre = graph.CreateUriNode(new Uri(predicate));
            var obj = graph.CreateUriNode(new Uri(@object));
            graph.Assert(new Triple(sub, pre, obj));
        }

        var record = default(Record);
        var result = () => record = new RecordBuilder()
            .WithId(id0)
            .WithScopes(scope)
            .WithDescribes(desc)
            .WithContent(graph.Triples.ToList())
            .Build();

        result.Should().NotThrow();

        record.Should().NotBeNull();
        for (var i = 0; i < numberOfTriples; i++)
        {
            var (subject, predicate, @object) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var quad = Quad.CreateSafe(subject, predicate, @object, id0);
            record.ContainsQuad(quad).Should().BeTrue();
        }
    }

    [Fact]
    public void RecordBuilder_With_RdfString()
    {
        var rdfString =
            @"<http://example.com/object/version/1234/5678> <http://www.w3.org/ns/prov#atLocation> <http://example.com/object/version/1234/5678/738499902> .
<http://example.com/object/version/1234/5678/738499902> <https://rdf.equinor.com/ontology/bravo-api#attachmentName> ""/scopeId=7f7bcbf0-b166-483e-8fd0-065991978824/year=2022/month=08/day=09/hour=13/minute=18/revisjon.png"" .
<http://example.com/object/version/1234/5678/738499902> <http://www.w3.org/1999/02/22-rdf-syntax-ns#type> <http://www.w3.org/ns/prov#Location> .
";
        var (s, p, o, g) = TestData.CreateRecordQuadStringTuple("0");
        var scope = TestData.CreateRecordIri("scope", "0");
        var desc = TestData.CreateRecordIri("describes", "0");

        var record = new RecordBuilder()
            .WithContent(rdfString)
            .WithScopes(scope)
            .WithId(g)
            .WithDescribes(desc)
            .Build();

        record.Id.Should().Be(g);
        record
            .QuadsWithSubject("http://example.com/object/version/1234/5678")
            .Count()
            .Should()
            .Be(1);
    }

    [Fact]
    public void RecordBuilder_Can_Replace_Content()
    {
        var id1 = TestData.CreateRecordId("1");

        var scope = TestData.CreateRecordIri("scope", "0");
        var desc = TestData.CreateRecordIri("describes", "0");

        var quads = new List<SafeQuad>();
        const int numberOfQuads = 10;
        for (var i = 0; i < numberOfQuads; i++)
        {
            var (subject, predicate, @object) = TestData.CreateRecordTripleStringTuple(i.ToString());
            var quad = Quad.CreateSafe(subject, predicate, @object, id1);
            quads.Add(quad);
        }

        var halfMark = numberOfQuads / 2;

        var builder = new RecordBuilder()
            .WithId(id1)
            .WithDescribes(desc)
            .WithScopes(scope)
            .WithContent(quads.GetRange(0, halfMark));

        var record1 = builder
            .WithContent(quads.GetRange(halfMark, numberOfQuads - halfMark))
            .Build();

        var record2 = builder
            .WithAdditionalContent(quads.GetRange(halfMark, numberOfQuads - halfMark))
            .Build();

        record1
            .Quads()
            .Should()
            .NotContain(quads.GetRange(0, halfMark));

        record2
            .Quads()
            .Should()
            .Contain(quads);
    }
}