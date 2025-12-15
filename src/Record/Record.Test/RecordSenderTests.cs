using FluentAssertions;
using System.Net.Http.Headers;
using VDS.RDF.Writing;
using Records.Sender;
using Records.Immutable;
using Microsoft.Azure.Functions.Worker.Http;
using System.Text;
using NSubstitute;
using Microsoft.Azure.Functions.Worker;

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
    public void Can_Add_RecordString_To_HttpRequestMessage()
    {
        var record = new RecordBuilder()
                        .WithId("https://example.com/record/1")
                        .WithScopes("https://example.com/scope/1")
                        .WithDescribes("https://example.com/object/1")
                        .Build();

        var message = new HttpRequestMessage();
        message.AddRecord(record.ToString<JsonLdWriter>());

        var resultContentType = message.Content!.Headers.ContentType;
        resultContentType.Should().Be(new MediaTypeHeaderValue("application/ld+json"));

        var resultStream = message.Content!.ReadAsStream();
        using var reader = new StreamReader(resultStream, System.Text.Encoding.UTF8);
        var resultContent = reader.ReadToEnd();

        resultContent.Should().Be(record.ToString<JsonLdWriter>());
    }

    [Fact]
    public void Can_Implicitly_Cast_Record_To_HttpRequestMessage()
    {
        var record = new RecordBuilder()
                        .WithId("https://example.com/record/1")
                        .WithScopes("https://example.com/scope/1")
                        .WithDescribes("https://example.com/object/1")
                        .Build();


        HttpRequestMessage message = record;

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

    [Fact]
    public void HttpRequestData_Can_Be_ParsedTo_Record()
    {
        var record = new RecordBuilder()
                .WithId("https://example.com/record/1")
                .WithScopes("https://example.com/scope/1")
                .WithDescribes("https://example.com/object/1")
                .Build();

        var recordBytes = Encoding.UTF8.GetBytes(record.ToString<JsonLdWriter>());

        // Mock Azure function and HttpRequestData
        var mockFunctionContext = Substitute.For<FunctionContext>();
        var requestData = Substitute.For<HttpRequestData>(mockFunctionContext);
        var bodyStream = new MemoryStream(recordBytes);
        requestData.Body.Returns(bodyStream);


        var recordRequest = requestData.ToRecord();
        recordRequest.Should().Be(record);
    }
}