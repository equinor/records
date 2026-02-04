using System.Net;
using System.Net.Http.Headers;
using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Writing.Formatting;

namespace Records.Backend;

public class FusekiRecordBackend : RecordBackendBase
{
    public readonly Uri baseAddress;
    public Uri SparqlEndpointUrl() => new($"{baseAddress}{_datasetName}/sparql");
    public Uri UpdateEndpointUrl() => new($"{baseAddress}{_datasetName}/update");
    private readonly Func<Task<string>>? _authorization;
    private const string _datasetName = "ds";

    private FusekiRecordBackend(Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        _authorization = authorization;
        this.baseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress), "Base address cannot be null.");
        InitializeMetadata();
    }

    public static FusekiRecordBackend CreateAsync(Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        var client = new FusekiRecordBackend(baseAddress, authorization);
        return client;
    }

    private async Task<HttpClient> UseClientAsync()
    {
        var client = new HttpClient { BaseAddress = SparqlEndpointUrl() };
        if (_authorization != null)
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await _authorization());

        return client;
    }

    internal async Task UploadRdfData(string recordId, string rdfData)
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

    internal async Task DeleteNamedGraphs(IEnumerable<string> graphUris)
    {
        using var httpClient = await UseClientAsync();
        foreach (var graphUri in graphUris)
        {
            var sparqlUpdate = $"DROP GRAPH <{graphUri}>";
            var content = new StringContent(sparqlUpdate, System.Text.Encoding.UTF8, "application/sparql-update");
            await httpClient.PostAsync(UpdateEndpointUrl(), content);
        }
    }

    internal async Task<SparqlQueryClient> GetSparqlQueryClient() =>
        new SparqlQueryClient(await UseClientAsync(), SparqlEndpointUrl());

    public async Task<HttpStatusCode> HealthCheck()
    {
        using var requestMessage = new HttpRequestMessage();
        requestMessage.Method = HttpMethod.Get;
        requestMessage.RequestUri = new Uri($"{baseAddress}Health");

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