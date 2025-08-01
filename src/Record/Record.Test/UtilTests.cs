using System.Text;
using FluentAssertions;
using Records.Exceptions;
using VDS.RDF.Writing;
using Records.Utils;
using Record = Records.Immutable.Record;
using VDS.RDF;
using VDS.RDF.Nodes;

namespace Records.Tests;

public class UtilTests
{
    [Fact]
    public void TimeUtils_CreateHasTimeTriples_Creates_Correct_Amount_Of_Triples()
    {
        var datetime = DateTime.Parse("2025-07-31");
        var parent = new BlankNode("Parent");

        var triples = TimeUtils.CreateHasTimeTriples(parent, datetime);

        triples.Count.Should().Be(5);
    }

    [Fact]
    public void TimeUtils_CreateHasTimeTriples_Creates_Correct_Month_UriNode()
    {
        var datetime = DateTime.Parse("2025-07-31");
        var parent = new BlankNode("Parent");

        var triples = TimeUtils.CreateHasTimeTriples(parent, datetime);

        var month = triples.Where(t => t.Predicate == Namespaces.Time.UriNodes.Month).Select(t => t.Object).Single();

        month.Should().Be(Namespaces.Greg.UriNodes.July);
    }

    [Fact]
    public void TimeUtils_CreateHasTimeTriples_Creates_Correct_Year_Literal()
    {
        var datetime = DateTime.Parse("2025-07-31");
        var parent = new BlankNode("Parent");

        var triples = TimeUtils.CreateHasTimeTriples(parent, datetime);

        var year = triples.Where(t => t.Predicate == Namespaces.Time.UriNodes.Year).Select(t => t.Object).Single();

        year.Should().BeOfType<LiteralNode>();

        var yearNode = year as LiteralNode;

        yearNode.Value.Should().Be(datetime.Year.ToString());
        yearNode.DataType.Should().Be(Namespaces.DataType.GYear);
    }

    [Fact]
    public void TimeUtils_CreateHasTimeTriples_Creates_Correct_Day_Literal()
    {
        var datetime = DateTime.Parse("2025-07-31");
        var parent = new BlankNode("Parent");

        var triples = TimeUtils.CreateHasTimeTriples(parent, datetime);

        var day = triples.Where(t => t.Predicate == Namespaces.Time.UriNodes.Day).Select(t => t.Object).Single();

        day.Should().BeOfType<LiteralNode>();

        var dayNode = day as LiteralNode;

        dayNode.Value.Should().Be($"---{datetime:dd}");
        dayNode.DataType.Should().Be(Namespaces.DataType.GDay);
    }
}
