using FluentAssertions;
using Records.Backend;
using System.Net;
using System.Reflection;

namespace Records.Tests;

public class FusekiRecordBackendRetryTests
{
    [Fact]
    public async Task CreateDatasetAsync_CreatesMemoryDatasetUsingFormParameters()
    {
        var handler = new ScriptedHttpMessageHandler(async request =>
        {
            request.Method.Should().Be(HttpMethod.Post);
            request.RequestUri!.AbsolutePath.Should().Be("/$/datasets");
            request.Content!.Headers.ContentType!.MediaType.Should().Be("application/x-www-form-urlencoded");

            var body = await request.Content.ReadAsStringAsync();
            var parameters = body.Split('&')
                .Select(parameter => parameter.Split('=', 2))
                .ToDictionary(parameter => parameter[0], parameter => Uri.UnescapeDataString(parameter[1]));
            parameters["dbName"].Should().StartWith("record_");
            Guid.TryParse(parameters["dbName"]["record_".Length..], out _).Should().BeTrue();
            parameters["dbType"].Should().Be("mem");

            return new HttpResponseMessage(HttpStatusCode.OK);
        });
        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var backend = CreateBackendForDatasetTests(httpClient);

        await backend.CreateDatasetAsync();
    }

    [Fact]
    public async Task CreateDatasetAsync_ShouldSucceed_WhenFirstAttemptTimesOutAndRetryConflicts()
    {
        var callCount = 0;
        var handler = new ScriptedHttpMessageHandler(_ =>
        {
            callCount++;

            return callCount switch
            {
                1 => throw new TaskCanceledException("Simulated timeout while creating dataset"),
                2 => new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent("Name already registered")
                },
                3 => new HttpResponseMessage(HttpStatusCode.OK),
                _ => throw new InvalidOperationException($"Unexpected call number: {callCount}")
            };
        });

        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var backend = CreateBackendForDatasetTests(httpClient);

        var retries = 0;
        Exception? lastException = null;
        while (retries < 2)
        {
            try
            {
                await backend.CreateDatasetAsync();
                lastException = null;
                break;
            }
            catch (TaskCanceledException ex)
            {
                lastException = ex;
                retries++;
            }
        }

        lastException.Should().BeNull("retrying a timed-out create should accept a subsequent conflict once dataset existence is verified");
        callCount.Should().Be(3, "one timeout, one retried create that conflicts, and one existence check");
    }

    [Fact]
    public async Task CreateDatasetAsync_ShouldFail_OnConflict_WhenDatasetCannotBeVerified()
    {
        var handler = new ScriptedHttpMessageHandler(request =>
        {
            if (request.Method == HttpMethod.Post)
            {
                return new HttpResponseMessage(HttpStatusCode.Conflict)
                {
                    Content = new StringContent("Name already registered")
                };
            }

            if (request.Method == HttpMethod.Get)
            {
                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent("Dataset not found")
                };
            }

            throw new InvalidOperationException($"Unexpected request method: {request.Method}");
        });

        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://localhost/") };
        var backend = CreateBackendForDatasetTests(httpClient);

        var act = async () => await backend.CreateDatasetAsync();
        var exception = await act.Should().ThrowAsync<Exception>();
        exception.Which.Message.Should().Contain("could not be verified");
    }

    private static FusekiRecordBackend CreateBackendForDatasetTests(HttpClient httpClient)
    {
        var constructor = typeof(FusekiRecordBackend)
            .GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, [typeof(HttpClient)], null)
            ?? throw new InvalidOperationException("Could not find non-public constructor for FusekiRecordBackend");

        return (FusekiRecordBackend)constructor.Invoke([httpClient]);
    }

    private sealed class ScriptedHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _responseFactory;

        public ScriptedHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responseFactory)
            : this(request => Task.FromResult(responseFactory(request))) { }

        public ScriptedHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => _responseFactory(request);
    }
}
