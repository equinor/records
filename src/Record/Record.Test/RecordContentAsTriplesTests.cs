using FluentAssertions;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Records.Tests;

public class RecordContentAsTriplesTests : IAsyncLifetime
{
    private const string Base = "https://bravo.equinor.com/monthlyprocurementpackagereportcollection/BOUVET/M.999A.999/1234567890/2025";
    private const string Subject04 = $"{Base}/04";
    private const string Subject05 = $"{Base}/05";
    private const string ContractType = "https://rdf.equinor.com/asset/Contract";
    private const string RdfType = "http://www.w3.org/1999/02/22-rdf-syntax-ns#type";
    private const string RdfsLabel = "http://www.w3.org/2000/01/rdf-schema#label";
    private const string Label = "1234567890^^http://www.w3.org/2001/XMLSchema#string";

    private List<Triple> _result = [];

    public async Task InitializeAsync()
    {
        var trig = await File.ReadAllTextAsync("Data/record-with-two-contents.trig");
        var record = await Immutable.Record.CreateAsync(trig, new TriGParser());
        _result = (await record.ContentAsTriples()).ToList();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public void Should_return_four_triples() =>
        _result.Should().HaveCount(4);

    [Fact]
    public void Should_contain_subject04_type_triple() =>
        _result.Should().ContainSingle(t =>
            t.Subject.ToString() == Subject04 &&
            t.Predicate.ToString() == RdfType &&
            t.Object.ToString() == ContractType);

    [Fact]
    public void Should_contain_subject04_label_triple() =>
        _result.Should().ContainSingle(t =>
            t.Subject.ToString() == Subject04 &&
            t.Predicate.ToString() == RdfsLabel &&
            t.Object.ToString() == Label);

    [Fact]
    public void Should_contain_subject05_type_triple() =>
        _result.Should().ContainSingle(t =>
            t.Subject.ToString() == Subject05 &&
            t.Predicate.ToString() == RdfType &&
            t.Object.ToString() == ContractType);

    [Fact]
    public void Should_contain_subject05_label_triple() =>
        _result.Should().ContainSingle(t =>
            t.Subject.ToString() == Subject05 &&
            t.Predicate.ToString() == RdfsLabel &&
            t.Object.ToString() == Label);
}