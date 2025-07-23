using FluentAssertions;
using Records.Exceptions;
using Record = Records.Immutable.Record;
using VDS.RDF;
using Newtonsoft.Json.Linq;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Shacl;
using VDS.RDF.Writing;
using static Records.ProvenanceBuilder;
using Xunit.Abstractions;
using Records.Utils;

namespace Records.Tests;
public class RecordBuilderTests
{
    private ITestOutputHelper _outputHelper;

    public RecordBuilderTests(ITestOutputHelper outputHelper) =>
        _outputHelper = outputHelper;

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
    public void Can_Add_Provenance()
    {
        var id = TestData.CreateRecordId("0");
        var scopes = TestData.CreateObjectList(2, "scope");
        var describes = TestData.CreateObjectList(2, "describes");
        var used = TestData.CreateObjectList(4, "used");
        var with = TestData.CreateObjectList(2, "with");
        var locations = TestData.CreateObjectList(2, "location");

        var record = new RecordBuilder()
            .WithScopes(scopes)
            .WithDescribes(describes)
            .WithAdditionalContentProvenance(
                WithAdditionalTool(with),
                WithAdditionalLocation(locations))
            .WithAdditionalMetadataProvenance(WithAdditionalUsing(used[3], used[2]))
            .WithAdditionalContentProvenance(WithAdditionalUsing(used[0], used[1]))
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

        result.Should().ThrowExactly<RecordException>().WithMessage("A record must either be a subrecord or have at least one scope");
    }

    [Fact]
    public void RecordBuilder_Does_Not_Merge_Blank_Nodes()
    {
        var rdfString = """

                        _:1G272G93_2024_01_Report_ActualAccumulatedCost a <https://rdf.equinor.com/cost/AccumulatedCost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            67825958.89;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_ActualPeriodicCost a <https://rdf.equinor.com/cost/PeriodicCost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            61778094.56;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_ApprovedAdjustments a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            58334293.92;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_ApprovedVO a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            94259910.57;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_EAC a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            1291122.2;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_ExecutedOptions a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            96414707.12;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_Growth a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            41711528.06;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_OriginalContractValue a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            24762931.17;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_PendingVORs a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            29625533.71;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        _:1G272G93_2024_01_Report_PotentialVORs a <https://rdf.equinor.com/cost/Cost>;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount>
                            74642027.39;
                          <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasCurrency>
                            <https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/ISO4217-CurrencyCodes/NOK> .

                        <https://assetid.equinor.com/contract/1111000011> a <https://rdf.equinor.com/asset/Contract>;
                          <http://www.w3.org/2000/01/rdf-schema#label> "1111000011" .

                        <https://assetid.equinor.com/contractorsWbs/1111000012/V8FXP3TMSJ> a <https://rdf.equinor.com/asset/ContractorsCostWbs>;
                          <http://www.w3.org/2000/01/rdf-schema#label> "V8FXP3TMSJ";
                          <https://rdf.equinor.com/asset/contractorsCostWBSDescription> "It's my WBS, is what it is." .

                        <https://assetid.equinor.com/project/1111000012> a <https://rdf.equinor.com/asset/Project>;
                          <http://www.w3.org/2000/01/rdf-schema#label> "1111000012" .

                        <https://assetid.equinor.com/workpack/1111000012/1G272G93> a <https://rdf.equinor.com/asset/Workpack>;
                          <http://www.w3.org/2000/01/rdf-schema#comment> "1G272G93 @ EX.01234A.001!";
                          <https://rdf.equinor.com/asset/partOfContract> <https://assetid.equinor.com/contract/1111000011>;
                          <https://rdf.equinor.com/asset/partOfProject> <https://assetid.equinor.com/project/1111000012>;
                          <https://rdf.equinor.com/asset/workpackId> "1G272G93";
                          <https://rdf.equinor.com/cost/hasReport> <https://assetid.equinor.com/workpack/1111000012/1G272G93/Report/2024/01> .

                        <https://assetid.equinor.com/workpack/1111000012/1G272G93/Report/2024/01> a <https://rdf.equinor.com/cost/Report>;
                          <http://www.w3.org/2000/01/rdf-schema#comment> "This lineitem is lit";
                          <http://www.w3.org/2000/01/rdf-schema#label> "Report for 1G272G93";
                          <http://www.w3.org/2006/time#inXSDgYearMonth> "2024-01"^^<http://www.w3.org/2001/XMLSchema#gYearMonth>;
                          <https://rdf.equinor.com/cost/hasContractorWbs> <https://assetid.equinor.com/contractorsWbs/1111000012/V8FXP3TMSJ>;
                          <https://rdf.equinor.com/cost/reportedActualAccumulatedCost> _:1G272G93_2024_01_Report_ActualAccumulatedCost;
                          <https://rdf.equinor.com/cost/reportedActualPeriodicCost> _:1G272G93_2024_01_Report_ActualPeriodicCost;
                          <https://rdf.equinor.com/cost/reportedApprovedAdjustments> _:1G272G93_2024_01_Report_ApprovedAdjustments;
                          <https://rdf.equinor.com/cost/reportedApprovedVariationOrder> _:1G272G93_2024_01_Report_ApprovedVO;
                          <https://rdf.equinor.com/cost/reportedEstimateAtComplete> _:1G272G93_2024_01_Report_EAC;
                          <https://rdf.equinor.com/cost/reportedExecutedOptions> _:1G272G93_2024_01_Report_ExecutedOptions;
                          <https://rdf.equinor.com/cost/reportedGrowth> _:1G272G93_2024_01_Report_Growth;
                          <https://rdf.equinor.com/cost/reportedOriginalContractValue> _:1G272G93_2024_01_Report_OriginalContractValue;
                          <https://rdf.equinor.com/cost/reportedPendingVariationOrderRequests> _:1G272G93_2024_01_Report_PendingVORs;
                          <https://rdf.equinor.com/cost/reportedPotentialVariationOrderRequests> _:1G272G93_2024_01_Report_PotentialVORs .

                        """;
        var recordId = $"https://rdf.equinor.com/MCR-{Guid.NewGuid()}";
        var contentrecordId = $"https://rdf.equinor.com/MCR-{Guid.NewGuid()}-Content";

        var graph = new Graph(new UriNode(new Uri(contentrecordId)));

        graph.LoadFromString(rdfString, new TurtleParser());

        var subjectsBefore = graph.GetTriplesWithPredicate(new UriNode(new Uri("https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount")));

        var scopes = new List<string>() { "https://example.com/scope/1", "https://example.com/scope/2" };

        var record = new RecordBuilder()
            .WithId(recordId)
            .WithScopes(scopes)
            .WithContent(graph.Triples)
            .WithDescribes("https://example.com/desc/1")
            .Build();

        var recordContentGraph = record.GetContentGraphs().First();
        var subjectsAfter = recordContentGraph.GetTriplesWithPredicate(new UriNode(new Uri("https://spec.edmcouncil.org/fibo/ontology/FND/Accounting/CurrencyAmount/hasAmount")))
            .Select(q => q.Subject)
            .Distinct()
            .Count();

        subjectsAfter.Should().Be(subjectsBefore.Count());
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
            record!.ContainsQuad(quad).Should().BeTrue();
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

    [Fact]
    public void RecordBuilder_Can_Add_IsSubRecordOf()
    {
        var id = TestData.CreateRecordId("1");
        var scope = TestData.CreateRecordIri("scope", "1");
        var describes = TestData.CreateRecordIri("describes", "1");

        var superRecordId = TestData.CreateRecordId("super");

        var content = Enumerable.Range(0, 10)
            .Select(i =>
            {
                var (s, p, o) = TestData.CreateRecordTripleStringTuple(i.ToString());
                return Quad.CreateSafe(s, p, o, id);
            })
            .ToList();

        var builder = new RecordBuilder()
            .WithId(id)
            .WithScopes(scope)
            .WithDescribes(describes)
            .WithContent(content)
            .WithIsSubRecordOf(superRecordId);

        var record = default(Record);
        var buildProcess = () => record = builder.Build();

        buildProcess.Should().NotThrow();
        record.Should().NotBeNull();

        record!.IsSubRecordOf.Should().Be(superRecordId);
        record.Id.Should().Be(id);
        record.Scopes.Should().Contain(scope);
        record.Describes.Should().Contain(describes);
        record.Quads().Should().Contain(content);
    }

    [Fact]
    public void RecordBuilder_Only_Adds_Latest_IsSubRecordOf()
    {
        var id = TestData.CreateRecordId("1");
        var scope = TestData.CreateRecordIri("scope", "1");
        var describes = TestData.CreateRecordIri("describes", "1");

        var superRecordId1 = TestData.CreateRecordId("super");
        var superRecordId2 = TestData.CreateRecordId("superer");

        var content = Enumerable.Range(0, 10)
            .Select(i =>
            {
                var (s, p, o) = TestData.CreateRecordTripleStringTuple(i.ToString());
                return Quad.CreateSafe(s, p, o, id);
            })
            .ToList();

        var builder = new RecordBuilder()
            .WithId(id)
            .WithScopes(scope)
            .WithDescribes(describes)
            .WithContent(content)
            .WithIsSubRecordOf(superRecordId1)
            .WithIsSubRecordOf(superRecordId2);

        var record = default(Record);
        var buildProcess = () => record = builder.Build();

        buildProcess.Should().NotThrow();
        record.Should().NotBeNull();

        record!.IsSubRecordOf.Should().Be(superRecordId2);
        record.Id.Should().Be(id);
        record.Scopes.Should().Contain(scope);
        record.Describes.Should().Contain(describes);
        record.Quads().Should().Contain(content);
    }

    [Fact]
    public void RecordBuilder_Builds_Object_Literals_Correctly_Check_With_JsonLd()
    {
        var graph = new Graph();
        var (s, p, _, g) = TestData.CreateRecordQuadStringTuple("1");
        graph.BaseUri = new Uri(g);

        var dateTypeUri = UriFactory.Create("http://www.w3.org/2001/XMLSchema#date");
        var stringTypeUri = UriFactory.Create($"http://www.w3.org/2001/XMLSchema#string");

        var subject = graph.CreateUriNode(new Uri(s));
        var predicate = graph.CreateUriNode(new Uri(p));
        var @object1 = graph.CreateLiteralNode("date", dateTypeUri);
        var @object2 = graph.CreateLiteralNode("string", stringTypeUri);

        graph.Assert(new Triple(subject, predicate, @object1));
        graph.Assert(new Triple(subject, predicate, @object2));

        var quads = graph.Triples.Select(triple => Quad.CreateSafe(triple, graph.BaseUri));

        var scopes = TestData.CreateObjectList(2, "scope");

        var record = new RecordBuilder()
            .WithId(g)
            .WithScopes(scopes)
            .WithContent(quads)
            .Build();

        record.Id.Should().Be(graph.BaseUri.ToString());

        var jsonLd = record.ToString<JsonLdWriter>();

        JArray.Parse(jsonLd)
            .SelectMany(jo => jo["@graph"]!.Children<JObject>())
            .SelectMany(jo => jo.Properties())
            .Any(jp =>
                jp.Name.Equals(p)
                && jp.Value is JArray ja
                && ja.Any(item =>
                    item["@type"]!.ToString() == dateTypeUri.ToString())
                )
            .Should()
            .BeTrue();
    }



    [Fact]
    public void RecordBuilder_Content_May_Not_Add_Provenance_Information()
    {
        var recordId = TestData.CreateRecordId("recordId");
        var superRecord = TestData.CreateRecordId("superRecordId");
        var superDuperRecord = TestData.CreateRecordId("superDuperRecordId");

        var record = default(Record);

        var recordBuilder = () =>
        {
            record = new RecordBuilder()
              .WithId(recordId)
              .WithIsSubRecordOf(superRecord)
              .WithContent(Quad.CreateSafe(recordId, Namespaces.Record.IsSubRecordOf, superDuperRecord, recordId))
              .Build();
        };

        recordBuilder.Should()
            .Throw<RecordException>()
            .WithMessage("Content may not make metadata statements.");

        record.Should().BeNull();
    }

    [Fact]
    public void RecordBuilder_Can_Add_ContentGraphs()
    {
        var firstGraph = TestData.CreateGraph("https://example.com/1");
        var secondGraph = TestData.CreateGraph("https://example.com/2");

        var record = TestData.RecordBuilderWithProvenanceAndWithoutContent()
            .WithContent(firstGraph)
            .WithAdditionalContent(secondGraph)
            .Build();

        record.GetContentGraphs().Should().HaveCount(2);
    }

    [Fact]
    public void RecordBuilder_Can_Add_Additional_Metadata()
    {
        var (s, p, _, _) = TestData.CreateRecordQuadStringTuple("1");
        var subject = new UriNode(new Uri(s));
        var predicate = new UriNode(new Uri(p));
        var @object = new LiteralNode("date", UriFactory.Create("http://www.w3.org/2001/XMLSchema#date"));
        var additionalMetadata = new Triple(subject, predicate, @object);

        var record = TestData.ValidRecordBeforeBuildComplete()
            .WithAdditionalMetadata(additionalMetadata)
            .Build();

        var metadataTriples = record.MetadataAsTriples();
        var contentTriples = record.ContentAsTriples();

        metadataTriples.Should().Contain(additionalMetadata);
        contentTriples.Should().NotContain(additionalMetadata);
    }

    [Fact]
    public void RecordBuilder_Fails_If_Subject_Is_Not_Record_Id_And_Predicate_Is_Record_Predicate()
    {
        var (s, _, _, g) = TestData.CreateRecordQuadStringTuple("1");
        var subject = new UriNode(new Uri(s));
        var predicate = Namespaces.Record.UriNodes.Describes;
        var @object = new LiteralNode("string", UriFactory.Create("http://www.w3.org/2001/XMLSchema#string"));
        var additionalMetadata = new Triple(subject, predicate, @object);

        var record = default(Record);

        var recordBuilder = () =>
        {
            record = TestData.ValidRecordBeforeBuildComplete()
                .WithAdditionalMetadata(additionalMetadata)
                .Build();
        };

        recordBuilder
            .Should()
            .Throw<RecordException>()
            .WithMessage("For all triples where the predicate is in the record ontology, the subject must be the record itself.");

        record.Should().BeNull();
    }


    [Fact]
    public void RecordBuilder__Hashes__ContentGraphs()
    {
        // Arrange 

        var contentGraphX = TestData.CreateGraph(TestData.CreateRecordId("contentX"), 1);
        var contentGraphY = TestData.CreateGraph(TestData.CreateRecordId("contentY"), 5);
        var hashX = CanonicalisationExtensions.HashGraph(contentGraphX);
        var hashY = CanonicalisationExtensions.HashGraph(contentGraphY);

        // Act
        var record = TestData.RecordBuilderWithProvenanceAndWithoutContent()
            .WithContent(contentGraphX)
            .WithAdditionalContent(contentGraphY)
            .Build();

        // Assert
        var contentGraphNames = record.GetContentGraphs().Select(g => g.Name);

        var query = new SparqlQueryParser().ParseFromString(
            @$"SELECT DISTINCT ?contentId ?checksumValue WHERE 
                    {{ 
                        GRAPH ?g {{ ?s ?p ?o . ?g a <{Namespaces.Record.RecordType}> .
                                    ?s <{Namespaces.Record.HasContent}> ?contentId . 
                                    ?contentId <{Namespaces.FileContent.HasChecksum}> ?checksum .
                                    ?checksum <{Namespaces.FileContent.HasChecksumValue}> ?checksumValue .
                                  }} 
                    }}");

        var ds = new InMemoryDataset((TripleStore)record.TripleStore());
        var qProcessor = new LeviathanQueryProcessor(ds);
        var qresults = (SparqlResultSet)qProcessor.ProcessQuery(query);
        var resultDict = qresults.Select(r =>
                (
                id: r["contentId"].ToString(),
                checksum: string.Join("", r["checksumValue"].ToString().TakeWhile(c => !c.Equals('^')))
                )).ToDictionary(tuple => tuple.id, tuple => tuple.checksum);

        resultDict[contentGraphX.Name.ToString()].Should().Be(hashX);
        resultDict[contentGraphY.Name.ToString()].Should().Be(hashY);
    }


    [Fact]
    public void RecordBuilder__CanBuild__RecordWithOnlyMetaDataGraph()
    {
        // Arrange 
        var recordBuilder = TestData.RecordBuilderWithProvenanceAndWithoutContent();

        // Act
        var record = recordBuilder.Build();

        // Assert
        record.MetadataGraph().Should().NotBeNull();
        record.GetContentGraphs().Should().BeEmpty();
    }
}