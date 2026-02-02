using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using VDS.RDF.Query;

namespace Records.Backend;

public class FusekiClient 
{
    public readonly Uri baseAddress;
    public Uri SparqlEndpointUrl() => new($"{baseAddress}{_datasetName}/sparql");
    public Uri UpdateEndpointUrl() => new($"{baseAddress}{_datasetName}/update");
    private readonly Func<Task<string>>? _authorization;
    private const string _datasetName = "ds";

    private FusekiClient(Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        _authorization = authorization;
        this.baseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress), "Base address cannot be null.");
    }

    public static FusekiClient CreateAsync(Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        var client = new FusekiClient(baseAddress, authorization);
        return client;
    }

    private async Task<HttpClient> UseClientAsync()
    {
        var client = new HttpClient { BaseAddress = SparqlEndpointUrl() };
        if (_authorization != null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authorization());

        return client;
    }

    public async Task UploadRdfData(string recordId, string rdfData)
    {
        using var client = await UseClientAsync();

        if (await RecordIsAlreadyUploaded(recordId))
            throw new Exception("Aborting: a record with this ID is already being processed by Fuseki.");

        var request = new HttpRequestMessage(HttpMethod.Post, $"{baseAddress}{_datasetName}/data");
        request.Content = new StringContent(rdfData, System.Text.Encoding.UTF8, "application/trig");

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload RDF data: {response.StatusCode} - {errorMessage}");
        }
    }

    private async Task<bool> RecordIsAlreadyUploaded(string recordId)
    {
        var ask = @$"ASK {{ 
                    GRAPH <{recordId}> {{ 
                        ?s ?p ?o . 
                    }}
                }}";

        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(ask);
        var result = sparqlResultSet.Result;

        return result;
    }

    public async Task DeleteNamedGraphs(IEnumerable<string> graphUris)
    {
        using var httpClient = await UseClientAsync();
        foreach (var graphUri in graphUris)
        {
            var sparqlUpdate = $"DROP GRAPH <{graphUri}>";
            var content = new StringContent(sparqlUpdate, System.Text.Encoding.UTF8, "application/sparql-update");
            await httpClient.PostAsync(UpdateEndpointUrl(), content);
        }
    }

    public async Task<SparqlQueryClient> GetSparqlQueryClient() =>
        new SparqlQueryClient(await UseClientAsync(), SparqlEndpointUrl());

    public async Task<HttpStatusCode> HealthCheck()
    {
        using var requestMessage = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{baseAddress}Health"),
        };

        var httpClient = await UseClientAsync();

        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(requestMessage);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("An error occurred during health check.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new Exception("The HTTP request was canceled or timed out during health check.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Something unexpected happened during health check.", ex);
        }

        return response.StatusCode;
    }
}