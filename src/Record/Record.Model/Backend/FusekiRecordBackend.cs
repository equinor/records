using System.Net;
using System.Net.Http.Headers;
using IriTools;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Writing.Formatting;

namespace Records.Backend;

public class FusekiRecordBackend : RecordBackendBase
{
    private readonly Uri BaseAddress;
    private Uri SparqlEndpointUrl() => new($"{BaseAddress}/{_datasetName}/sparql");
    private Uri UpdateEndpointUrl() => new($"{BaseAddress}/{_datasetName}/update");
    private Uri DataEndpointUrl() => new($"{BaseAddress}/{_datasetName}/data");
    private Uri CreateDatasetEndpointUrl() => new($"{BaseAddress}/$/datasets");
    private Uri DatasetEndpointUrl() => new($"{BaseAddress}/$/datasets/{_datasetName}");
    private readonly Func<Task<string>>? _authorization;
    private readonly string _datasetName;

    private FusekiRecordBackend(Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        _datasetName = $"record_{Guid.NewGuid()}";
        _authorization = authorization;
        BaseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress), "Base address cannot be null.");
    }


    public static async Task<FusekiRecordBackend> CreateAsync(string rdfString, Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        var client = new FusekiRecordBackend(baseAddress, authorization);
        await client.CreateDatasetAsync();
        await client.UploadRdfData(rdfString);
        await client.InitializeMetadata();
        return client;
    }

    private async Task<HttpClient> CreateSparqlClientAsync()
    {
        var client = new HttpClient { BaseAddress = SparqlEndpointUrl() };
        if (_authorization != null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authorization());

        return client;
    }
    private async Task<HttpClient> CreateClientAsync()
    {
        var client = new HttpClient { };
        if (_authorization != null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authorization());

        return client;
    }

    private async Task CreateDatasetAsync()
    {
        var client = await CreateClientAsync();
        var query = $"dbName={_datasetName}&dbType=memory";
        var fullUri = $"{CreateDatasetEndpointUrl()}?{query}";
        var content = new StringContent("");
        var response = await client.PostAsync(fullUri, content);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create dataset: {response.StatusCode} - {errorMessage}");
        }
    }

    private async Task DeleteDatasetAsync()
    {
        var client = await CreateClientAsync();
        var jsonContent = $"{{\"name\": \"{_datasetName}\", \"type\": \"memory\"}}";
        var reqContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await client.DeleteAsync(DataEndpointUrl());
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create dataset: {response.StatusCode} - {errorMessage}");
        }
    }

    internal async Task UploadRdfData(string rdfData)
    {
        using var client = await CreateClientAsync();
        var request = new HttpRequestMessage(HttpMethod.Post, DataEndpointUrl());
        request.Content = new StringContent(rdfData, System.Text.Encoding.UTF8, "application/trig");
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload RDF data: {response.StatusCode} - {errorMessage}");
        }
    }

    internal async Task DeleteNamedGraphs(IEnumerable<string> graphUris)
    {
        using var httpClient = await CreateSparqlClientAsync();
        foreach (var graphUri in graphUris)
        {
            var sparqlUpdate = $"DROP GRAPH <{graphUri}>";
            var content = new StringContent(sparqlUpdate, System.Text.Encoding.UTF8, "application/sparql-update");
            await httpClient.PostAsync(UpdateEndpointUrl(), content);
        }
    }

    internal async Task<SparqlQueryClient> GetSparqlQueryClient() =>
        new SparqlQueryClient(await CreateSparqlClientAsync(), SparqlEndpointUrl());

    public async Task<HttpStatusCode> HealthCheck()
    {
        using var requestMessage = new HttpRequestMessage();
        requestMessage.Method = HttpMethod.Get;
        requestMessage.RequestUri = new Uri($"{BaseAddress}Health");

        var httpClient = await CreateSparqlClientAsync();

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

    public override Task<ITripleStore> TripleStore()
    {
        throw new NotImplementedException();
    }

    public override Task<string> ToString(IStoreWriter writer)
    {
        throw new NotImplementedException();
    }

    public override async Task<IEnumerable<INode>> SubjectWithType(UriNode type)
    {
        string x = "x";
        var queryBuilder = QueryBuilder
            .Select(new string[] { x })
            .Where(triplePatternBuilder =>
            {
                triplePatternBuilder
                    .Subject(x)
                    .PredicateUri(new Uri("http://www.w3.org/2000/01/rdf-schema#type"))
                    .Object(type.Uri);
            });
        var queryString = queryBuilder.BuildQuery().ToString();
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result => new UriNode(new Uri(result.Value(x).ToString(new TurtleFormatter()))));
    }

    public override Task<IEnumerable<string>> LabelsOfSubject(UriNode subject)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> TriplesWithPredicate(UriNode predicate)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> TriplesWithObject(INode @object)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(UriNode predicate, INode @object)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> TriplesWithSubjectObject(UriNode subject, INode @object)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate)
    {
        throw new NotImplementedException();
    }

    public override async Task<IGraph> ConstructQuery(SparqlQuery query)
    {
        var queryClient = await GetSparqlQueryClient();
        return await queryClient.QueryWithResultGraphAsync(query.ToString());
    }

    public override Task<SparqlResultSet> Query(SparqlQuery query)
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<string>> Sparql(string queryString)
    {
        throw new NotImplementedException();
    }

    public override Task<IGraph> GetMergedGraphs()
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<IGraph>> GetContentGraphs()
    {
        throw new NotImplementedException();
    }

    public override Task<IEnumerable<Triple>> Triples()
    {
        throw new NotImplementedException();
    }

    public override Task<bool> ContainsTriple(Triple triple)
    {
        throw new NotImplementedException();
    }

    public override Task<string> ToCanonString()
    {
        throw new NotImplementedException();
    }
}