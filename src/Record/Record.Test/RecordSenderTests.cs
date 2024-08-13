using FluentAssertions;
using System.Net.Http.Headers;
using VDS.RDF.Writing;
using Records.Sender;
using Records.Immutable;

namespace Records.Tests;

public class RecordSenderTests
{
    [Fact]
    public void Can_Add_Record_To_HttpRequestMessage()
    {
        var record = new RecordBuilder()
                        .WithId("https://example.com/record/1")
                        .WithScopes("https://example.com/scope/1")
                        .WithDescribes("https://example.com/object/1")
                        .Build();

        var message = new HttpRequestMessage();
        message.AddRecord(record);

        var resultContentType = message.Content!.Headers.ContentType;
        resultContentType.Should().Be(new MediaTypeHeaderValue("application/ld+json"));

        var resultStream = message.Content!.ReadAsStream();
        using var reader = new StreamReader(resultStream, System.Text.Encoding.UTF8);
        var resultContent = reader.ReadToEnd();

        resultContent.Should().Be(record.ToString<JsonLdWriter>());
    }

    [Fact]
    public void Can_Add_RecordId_To_HttpRequestMessage()
    {
        var recordId = "https://example.com/record/1";
        var record = new RecordBuilder()
                .WithId(recordId)
                .WithScopes("https://example.com/scope/1")
                .WithDescribes("https://example.com/object/1")
                .Build();

        var message = new HttpRequestMessage();
        message.AddRecordId(record);

        var resultStream = message.Content!.ReadAsStream();
        using var reader = new StreamReader(resultStream, System.Text.Encoding.UTF8);
        var resultContent = reader.ReadToEnd();

        resultContent.Should().Contain(recordId);
    }

    [Fact]
    public void Can_Build_RecordSender()
    {
        var record = new RecordBuilder()
                .WithId("https://example.com/record/1")
                .WithScopes("https://example.com/scope/1")
                .WithDescribes("https://example.com/object/1")
                .Build();

        var message = new RecordMessageBuilder("token")
                        .ForEndpoint("https://example.com/record")
                        .WithRecord(record)
                        .WithCursor(2)
                        .Build(RecordMessageBuilder.RecordOperation.Send);

        message.Should().BeOfType<HttpRequestMessage>();
        message.Method.Should().Be(HttpMethod.Post);

        message.RequestUri!.ToString().Should().Contain("?cursor");

        var resultContentType = message.Content!.Headers.ContentType;
        resultContentType.Should().Be(new MediaTypeHeaderValue("application/ld+json"));

        var resultStream = message.Content!.ReadAsStream();
        using var reader = new StreamReader(resultStream, System.Text.Encoding.UTF8);
        var resultContent = reader.ReadToEnd();

        resultContent.Should().Be(record.ToString<JsonLdWriter>());

    }

    [Fact]
    public void Can_Build_RecordRetracter()
    {
        var record = new RecordBuilder()
                .WithId("https://example.com/record/1")
                .WithScopes("https://example.com/scope/1")
                .WithDescribes("https://example.com/object/1")
                .Build();

        var message = new RecordMessageBuilder()
                        .WithToken("token")
                        .ForEndpoint("https://example.com/record")
                        .WithRecord(record)
                        .Build(RecordMessageBuilder.RecordOperation.Retract);

        message.Should().BeOfType<HttpRequestMessage>();
        message.Method.Should().Be(HttpMethod.Delete);

        var resultStream = message.Content!.ReadAsStream();
        using var reader = new StreamReader(resultStream, System.Text.Encoding.UTF8);
        var resultContent = reader.ReadToEnd();

        resultContent.Should().Contain(record.Id);
    }

    [Fact]
    public void Fail_Build_RecordSender()
    {
        var record = new RecordBuilder()
        .WithId("https://example.com/record/1")
        .WithScopes("https://example.com/scope/1")
        .WithDescribes("https://example.com/object/1")
        .Build();

        var result = () => new RecordMessageBuilder()
                        .WithToken("token")
                        .ForEndpoint("https://example.com/record")
                        .WithRecord(record)
                        .Build(RecordMessageBuilder.RecordOperation.Send);

        result.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Record_Can_Create_Builder()
    {
        var record = new RecordBuilder()
                .WithId("https://example.com/record/1")
                .WithScopes("https://example.com/scope/1")
                .WithDescribes("https://example.com/object/1")
                .Build();

        var message = record.CreateRecordMessageBuilder("token")
                        .ForEndpoint("https://example.com/record")
                        .WithRecord(record)
                        .WithCursor(2)
                        .Build(RecordMessageBuilder.RecordOperation.Send);

        message.Should().BeOfType<HttpRequestMessage>();
        message.Method.Should().Be(HttpMethod.Post);

        message.RequestUri!.ToString().Should().Contain("?cursor");

        var resultContentType = message.Content!.Headers.ContentType;
        resultContentType.Should().Be(new MediaTypeHeaderValue("application/ld+json"));

        var resultStream = message.Content!.ReadAsStream();
        using var reader = new StreamReader(resultStream, System.Text.Encoding.UTF8);
        var resultContent = reader.ReadToEnd();

        resultContent.Should().Be(record.ToString<JsonLdWriter>());
    }
}