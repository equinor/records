﻿using FluentAssertions;
using Records;
using Records.Exceptions;
using VDS.RDF;

namespace Records.Tests;

public class QuadTests
{
    [Fact]
    public void QuadBuilder()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var quad = Quad.CreateBuilder()
            .WithSubject(subject)
            .WithPredicate(predicate)
            .WithObject(@object)
            .WithGraphLabel(graphLabel)
            .Build();

        var (s, p, o, g) = quad;

        s.Should().Be(subject);
        p.Should().Be(predicate);
        o.Should().Be(@object);
        g.Should().Be(graphLabel);
    }

    [Fact]
    public void Quad_Deconstruction()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var quad = Quad.CreateSafe(subject, predicate, @object, graphLabel);

        var (s, p, o, g) = quad;

        s.Should().Be(subject);
        p.Should().Be(predicate);
        o.Should().Be(@object);
        g.Should().Be(graphLabel);
    }

    [Fact]
    public void Quad_Stringified()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var quad = Quad.CreateSafe(subject, predicate, @object, graphLabel);
        var result = quad.ToString();

        result.Should().Be($"<{subject}> <{predicate}> <{@object}> <{graphLabel}> .");
    }

    [Fact]
    public void Quad_From_Triple()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var graph = new Graph();
        graph.BaseUri = new Uri(graphLabel);
        var graphSubject = graph.CreateUriNode(new Uri(subject));
        var graphPredicate = graph.CreateUriNode(new Uri(predicate));
        var graphObject = graph.CreateUriNode(new Uri(@object));

        graph.Assert(new Triple(graphSubject, graphPredicate, graphObject));

        var quad = Quad.CreateSafe(graph.Triples.First(), graph.BaseUri);

        var (s, p, o, g) = quad;

        s.Should().Be(subject);
        p.Should().Be(predicate);
        o.Should().Be(@object);
        g.Should().Be(graphLabel);
    }

    [Fact]
    public void QuadBuilder_Can_Build_From_Triple()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var quad = Quad.CreateBuilder()
            .WithStatement($"<{subject}> <{predicate}> <{@object}> .")
            .WithGraphLabel(graphLabel)
            .Build();

        var (s, p, o, g) = quad;

        s.Should().Be(subject);
        p.Should().Be(predicate);
        o.Should().Be(@object);
        g.Should().Be(graphLabel);
    }

    [Fact]
    public void Create_UnsafeQuad()
    {
        var quad = Quad.CreateUnsafe("s", "p", "o", "g");

        quad.Should().BeOfType<UnsafeQuad>();
    }

    [Fact]
    public void QuadBuilder_Fluent()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var quad = new QuadBuilder()
            .WithGraphLabel(graphLabel)
            .WithSubject(subject)
            .WithPredicate(predicate)
            .WithObject(@object)
            .Build();

        quad.Should().BeOfType<SafeQuad>();
        quad.GraphLabel.Should().Be(graphLabel);
        quad.Subject.Should().Be(subject);
        quad.Predicate.Should().Be(predicate);
        quad.Object.Should().Be(@object);
    }

    [Fact]
    public void QuadBuilder_Fluent_Multiple()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("1");

        var @object1 = TestData.CreateRecordObject("1");

        var builder = new QuadBuilder()
            .WithGraphLabel(graphLabel)
            .WithSubject(subject)
            .WithPredicate(predicate)
            .WithObject(@object);

        var builder1 = builder.WithObject(@object1);

        var quad = builder.Build();
        var quad1 = builder1.Build();

        quad.Should().BeOfType<SafeQuad>();
        quad.GraphLabel.Should().Be(graphLabel);
        quad.Subject.Should().Be(subject);
        quad.Predicate.Should().Be(predicate);
        quad.Object.Should().Be(@object);

        quad1.Should().BeOfType<SafeQuad>();
        quad1.GraphLabel.Should().Be(graphLabel);
        quad1.Subject.Should().Be(subject);
        quad1.Predicate.Should().Be(predicate);
        quad1.Object.Should().Be(@object1);
    }

    [Fact]
    public void QuadBuilder_Fails_Fast_On_GraphLabel()
    {
        var result = () =>
        {
            var quad = new QuadBuilder()
                .WithGraphLabel("graphlabel")
                .WithSubject("subject")
                .WithPredicate("predicate")
                .WithObject("object")
                .Build();
        };

        result.Should().ThrowExactly<QuadException>().WithMessage("Failure in quad. Failed to parse graph label.");
    }

    [Fact]
    public void QuadBuilder_Premature_Build()
    {
        var result = () =>
        {
            var quad = new QuadBuilder()
                .WithGraphLabel(TestData.CreateRecordId("0"))
                .Build();
        };

        result.Should().ThrowExactly<QuadException>().WithMessage("Failure in quad. Missing one or more properties.");
    }

    [Fact]
    public void SafeQuad_Can_Be_Cloned_With_New_Value()
    {
        var (subject, predicate, @object, graphLabel) = TestData.CreateRecordQuadStringTuple("0");

        var quad = Quad.CreateBuilder()
            .WithSubject(subject)
            .WithPredicate(predicate)
            .WithObject(@object)
            .WithGraphLabel(graphLabel)
            .Build();

        quad.Subject.Should().Be(subject);
        quad.Predicate.Should().Be(predicate);
        quad.Object.Should().Be(@object);
        quad.GraphLabel.Should().Be(graphLabel);

        var newId = TestData.CreateRecordId("1");
        var newQuad = quad.WithGraphLabel(newId);

        newQuad.Subject.Should().Be(subject);
        newQuad.Predicate.Should().Be(predicate);
        newQuad.Object.Should().Be(@object);
        newQuad.GraphLabel.Should().Be(newId);

        quad.Should().NotBe(newQuad);
        quad.GraphLabel.Should().Be(graphLabel);
    }

    [Fact]
    public void UnsafeQuad_Can_Be_Cloned_With_New_Value()
    {
        var (subject, predicate, @object, id) = TestData.CreateRecordQuadStringTuple("0");

        var unsafeQuad = Quad.CreateUnsafe(subject, predicate, @object, id);

        unsafeQuad.Subject.Should().Be(subject);
        unsafeQuad.Predicate.Should().Be(predicate);
        unsafeQuad.Object.Should().Be(@object);
        unsafeQuad.GraphLabel.Should().Be(id);

        var newId = TestData.CreateRecordId("1");
        var newUnsafeQuad = unsafeQuad.WithGraphLabel(newId);

        unsafeQuad.Should().NotBe(newUnsafeQuad);
        unsafeQuad.GraphLabel.Should().Be(id);

        newUnsafeQuad.GraphLabel.Should().Be(newId);
        newUnsafeQuad.Subject.Should().Be(subject);
        newUnsafeQuad.Predicate.Should().Be(predicate);
        newUnsafeQuad.Object.Should().Be(@object);
    }
}
