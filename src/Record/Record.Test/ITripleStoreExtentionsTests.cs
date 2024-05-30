using FluentAssertions;
using VDS.RDF;

namespace Records.Tests;

public class ITripleStoreExtentionsTests
{
    [Fact]
    public void Collapse_WithId_ShouldReturnGraph()
    {
        // Arrange
        var store = new TripleStore();
        var id = new UriNode(new Uri("urn:default"));
        var graph = new Graph(id);
        store.Add(graph);

        // Act
        var result = store.Collapse(id);

        // Assert
        result.Should().BeEquivalentTo(graph);
    }

    [Fact]
    public void Collapse_WithUri_ShouldReturnGraph()
    {
        // Arrange
        var store = new TripleStore();
        var id = new Uri("urn:default");
        var graph = new Graph(new UriNode(id));
        store.Add(graph);

        // Act
        var result = store.Collapse(id);

        // Assert
        result.Should().BeEquivalentTo(graph);
    }

    [Fact]
    public void Collapse_WithIRefNode_ShouldReturnGraph()
    {
        // Arrange
        var store = new TripleStore();
        var id = new UriNode(new Uri("urn:default"));
        var graph = new Graph(id);
        store.Add(graph);

        // Act
        var result = store.Collapse(graph.Name);

        // Assert
        result.Should().BeEquivalentTo(graph);
    }
    [Fact]
    public void Collapse_WithMultipleGraphs_ShouldReturnGraph()
    {
        // Arrange
        var firstId = "https://example.com/1";
        var secondId = "https://example.com/2";

        var tripleStore = new TripleStore();

        var firstGraph = TestData.CreateGraph(firstId);
        var secondGraph = TestData.CreateGraph(secondId);

        tripleStore.Add(firstGraph);
        tripleStore.Add(secondGraph);

        // Act
        var result = tripleStore.Collapse(firstId);

        // Assert
        result.Triples.Should().HaveCount(tripleStore.Triples.Count());
        tripleStore.Triples.Should().Contain(tripleStore.Triples);
    }

}
