using System.Net;
using System.Net.Http.Headers;
using System.Text;
using IriTools;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;

namespace Records.Backend;

public class FusekiRecordBackend : RecordBackendBase
{
    private readonly Uri BaseAddress;
    private Uri SparqlEndpointUrl() => new($"{BaseAddress}{_datasetName}/sparql");
    private Uri UpdateEndpointUrl() => new($"{BaseAddress}{_datasetName}/update");
    private Uri DataEndpointUrl() => new($"{BaseAddress}{_datasetName}/data");
    private Uri CreateDatasetEndpointUrl() => new($"{BaseAddress}$/datasets");
    private Uri DatasetEndpointUrl() => new($"{BaseAddress}$/datasets/{_datasetName}");
    private readonly Func<Task<string>>? _authorization;
    private readonly string _datasetName;

    private FusekiRecordBackend(Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        _datasetName = $"record_{Guid.NewGuid()}";
        _authorization = authorization;
        BaseAddress = baseAddress ?? throw new ArgumentNullException(nameof(baseAddress), "Base address cannot be null.");
    }
    public static Task<FusekiRecordBackend> CreateFromTrigAsync(string rdfString, Uri baseAddress, Func<Task<string>>? authorization = null) =>
        CreateAsync(rdfString, RdfMediaType.Trig, baseAddress, authorization);

    public static Task<FusekiRecordBackend> CreateFromJsonLdAsync(string rdfString, Uri baseAddress, Func<Task<string>>? authorization = null) =>
        CreateAsync(rdfString, RdfMediaType.JsonLd, baseAddress, authorization);

    public static Task<FusekiRecordBackend> CreateFromNQuadsAsync(string rdfString, Uri baseAddress, Func<Task<string>>? authorization = null) =>
        CreateAsync(rdfString, RdfMediaType.Quads, baseAddress, authorization);


    public static async Task<FusekiRecordBackend> CreateAsync(string rdfString, RdfMediaType contentType, Uri baseAddress, Func<Task<string>>? authorization = null)
    {
        var client = new FusekiRecordBackend(baseAddress, authorization);
        await client.CreateDatasetAsync();
        await client.UploadRdfData(rdfString, contentType);
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
        var query = $"dbName={_datasetName}&dbType=mem";
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

    internal async Task UploadRdfData(string rdfData, RdfMediaType contentType)
    {
        using var client = await CreateClientAsync();
        if (contentType == RdfMediaType.JsonLd)
            ValidateJsonLd(rdfData);
        var request = new HttpRequestMessage(HttpMethod.Post, DataEndpointUrl());
        request.Content = new StringContent(rdfData, contentType.GetMediaTypeHeaderValue());

        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload RDF data: {response.StatusCode} - {errorMessage}");
        }
    }

    internal async Task<SparqlQueryClient> GetSparqlQueryClient() =>
        new SparqlQueryClient(await CreateSparqlClientAsync(), SparqlEndpointUrl());


    public override async Task<ITripleStore> TripleStore()
    {
        var content = await GetRdfDataAsString(RdfMediaType.Quads);
        var ts = new TripleStore();
        var parser = new VDS.RDF.Parsing.NQuadsParser();
        parser.Load(ts, content);
        return ts;
    }

    public override Task<string> ToString(RdfMediaType mediaType)
    {
        return GetRdfDataAsString(mediaType);
    }

    internal async Task<string> GetRdfDataAsString(RdfMediaType mediaType)
    {
        using var client = await CreateClientAsync();
        var request = new HttpRequestMessage(HttpMethod.Get, DataEndpointUrl());
        request.Headers.Accept.Add(mediaType.GetMediaTypeWithQualityHeaderValue());
        var response = await client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve RDF data: {response.StatusCode} - {errorMessage}");
        }
        var fusekiDatasetReponse = await response.Content.ReadAsStringAsync();
        if(mediaType == RdfMediaType.JsonLd)
            ValidateJsonLd(fusekiDatasetReponse);
        return fusekiDatasetReponse;
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
        return sparqlResultSet.Select(result => result.Value(x));
    }

    public override async Task<IEnumerable<string>> LabelsOfSubject(UriNode subject)
    {
        string queryString = $"SELECT ?label WHERE {{ GRAPH ?g {{ {subject.ToString(new TurtleFormatter())} rdfs:label ?label . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            result.Value("label").ToString(new TurtleFormatter())
        );
    }



    public override async Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject)
    {
        string queryString = $"SELECT ?p ?o WHERE {{ GRAPH ?g {{ {subject.ToString(new TurtleFormatter())} ?p ?o . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(subject,
                result.Value("p"),
                result.Value("o")
            ));
    }

    public override async Task<IEnumerable<Triple>> TriplesWithPredicate(UriNode predicate)
    {
        string queryString = $"SELECT ?s ?o WHERE {{ GRAPH ?g {{ ?s {predicate.ToString(new TurtleFormatter())} ?o . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(result.Value("s"),
                predicate,
                result.Value("o")
            ));
    }

    public override async Task<IEnumerable<Triple>> TriplesWithObject(INode @object)
    {
        string queryString = $"SELECT ?s ?p WHERE {{ GRAPH ?g {{ ?s ?p {@object.ToString(new TurtleFormatter())} . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(result.Value("s"),
                result.Value("p"),
                @object
            ));
    }

    public override async Task<IEnumerable<Triple>> TriplesWithPredicateAndObject(UriNode predicate, INode @object)
    {
        var turtleFormatter = new TurtleFormatter();
        string queryString = $"SELECT ?s WHERE {{ GRAPH ?g {{ ?s {predicate.ToString(turtleFormatter)} {@object.ToString(turtleFormatter)} . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(result.Value("s"),
                predicate,
                @object
            ));
    }

    public override async Task<IEnumerable<Triple>> TriplesWithSubjectObject(UriNode subject, INode @object)
    {
        var turtleFormatter = new TurtleFormatter();
        string queryString = $"SELECT ?p WHERE {{ GRAPH ?g{{ {subject.ToString(turtleFormatter)} ?p {@object.ToString(turtleFormatter)} . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(subject,
                result.Value("p"),
                @object
            ));
    }

    public override async Task<IEnumerable<Triple>> TriplesWithSubjectPredicate(UriNode subject, UriNode predicate)
    {
        var turtleFormatter = new TurtleFormatter();
        string queryString = $"SELECT ?o WHERE {{ {subject.ToString(turtleFormatter)} {predicate.ToString(turtleFormatter)} ?o . }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(subject,
                predicate,
                result.Value("o")
            ));
    }

    public override async Task<IGraph> ConstructQuery(SparqlQuery query)
    {
        var queryClient = await GetSparqlQueryClient();
        return await queryClient.QueryWithResultGraphAsync(query.ToString());
    }

    public override async Task<SparqlResultSet> Query(SparqlQuery query)
    {
        var queryClient = await GetSparqlQueryClient();
        return await queryClient.QueryWithResultSetAsync(query.ToString());
    }

    public override async Task<IEnumerable<string>> Sparql(string queryString)
    {
        var queryClient = await GetSparqlQueryClient();
        var command = queryString.Split().First();
        return command.ToLower() switch
        {
            "construct" => (await queryClient.QueryWithResultGraphAsync(queryString))
                .Triples
                .Select(tr => tr.ToString(new TurtleFormatter())),
            "select" =>(await queryClient.QueryWithResultSetAsync(queryString)).Results.Select(result =>
                result.ToString() ?? throw new InvalidOperationException("Null result from sparql query on record")),
            _ => throw new ArgumentException("Unsupported command in SPARQL query.")
        };
    }
        
    

    public override async Task<IGraph> GetMergedGraphs()
    {
        var queryClient = await GetSparqlQueryClient();
        return await queryClient.QueryWithResultGraphAsync("CONSTRUCT { ?s ?p ?o . } WHERE { GRAPH ?g { ?s ?p ?o . } }");
    }

    public override async Task<IEnumerable<IGraph>> GetContentGraphs()
    {
        var ts = await TripleStore();
        return ts.Graphs;
    }

    public override async Task<IEnumerable<Triple>> Triples()
    {
        string queryString = $"SELECT ?s ?p ?o WHERE {{ GRAPH ?g {{ ?s ?p ?o . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(result.Value("s"),
                result.Value("p"),
                result.Value("o")
            ));
    }

    public override async Task<bool> ContainsTriple(Triple triple)
    {
        var queryString = $"ASK WHERE {{ GRAPH ?g {{ {triple.Subject.ToString(new TurtleFormatter())} {triple.Predicate.ToString(new TurtleFormatter())} {triple.Object.ToString(new TurtleFormatter())} . }} }}";
        var queryClient = await GetSparqlQueryClient();
        var qResult = await queryClient.QueryWithResultSetAsync(queryString);
        return qResult.Result;
    }

    public override Task<string> ToCanonString()
    {
        //TODO
        throw new NotImplementedException();
    }
}