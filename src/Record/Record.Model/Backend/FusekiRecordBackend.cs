using Records.Immutable;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using StringWriter = System.IO.StringWriter;

namespace Records.Backend;

public class FusekiRecordBackend : RecordBackendBase
{
    private readonly HttpClient _httpClient;
    private readonly Uri _baseUri;
    private Uri SparqlEndpointUri() => new($"{_baseUri}{_datasetName}/sparql");
    private string UpdateEndpointPath() => new($"{_datasetName}/update");
    private string DataEndpointPath() => new($"{_datasetName}/data");
    private string CreateDatasetEndpointPath() => new($"$/datasets");
    private string DatasetEndpointPath() => new($"$/datasets/{_datasetName}");
    private readonly string _datasetName;

    private FusekiRecordBackend(HttpClient httpClient)
    {
        _datasetName = $"record_{Guid.NewGuid()}";
        _httpClient = httpClient;
        _baseUri = httpClient.BaseAddress ?? throw new InvalidOperationException("The HttpClient parameter must have a BaseAddress set.");
    }
    public static Task<FusekiRecordBackend> CreateFromTrigAsync(string rdfString, HttpClient httpClient) =>
        CreateAsync(rdfString, RdfMediaType.Trig, httpClient);

    public static Task<FusekiRecordBackend> CreateFromJsonLdAsync(string rdfString, HttpClient httpClient) =>
        CreateAsync(rdfString, RdfMediaType.JsonLd, httpClient);

    public static Task<FusekiRecordBackend> CreateFromNQuadsAsync(string rdfString, HttpClient httpClient) =>
        CreateAsync(rdfString, RdfMediaType.Quads, httpClient);


    public static async Task<FusekiRecordBackend> CreateAsync(string rdfString, RdfMediaType contentType, HttpClient httpClient)
    {
        var client = new FusekiRecordBackend(httpClient);
        await client.CreateDatasetAsync();
        await client.UploadRdfData(rdfString, contentType);
        await client.InitializeMetadata();
        return client;
    }


    private async Task CreateDatasetAsync()
    {
        var query = $"dbName={_datasetName}&dbType=mem";
        var fullUri = $"{CreateDatasetEndpointPath()}?{query}";
        var content = new StringContent("");
        var response = await _httpClient.PostAsync(fullUri, content);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create dataset: {response.StatusCode} - {errorMessage}");
        }
    }

    public override async ValueTask DeleteDatasetAsync()
    {
        var jsonContent = $"{{\"name\": \"{_datasetName}\", \"type\": \"memory\"}}";
        var reqContent = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.DeleteAsync(DatasetEndpointPath());
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to create dataset: {response.StatusCode} - {errorMessage}");
        }
    }

    public override async Task<IRecordBackend> WithAdditionalMetadata(IGraph additionalMetadata)
    {
        var originalRecordString = await GetRdfDataAsString(RdfMediaType.Quads);

        var ts = new TripleStore();
        var metadataGraph = new Graph(RecordId);
        metadataGraph.Assert(additionalMetadata.Triples);
        ts.Add(metadataGraph);
        var nquadsWriter = new NQuadsWriter();
        var stringWRiter = new StringWriter();
        nquadsWriter.Save(ts, stringWRiter);
        var newRecordString = stringWRiter.ToString();
        var combinedRecordString = $"{originalRecordString}\n{newRecordString}";
        return await FusekiRecordBackend.CreateAsync(combinedRecordString, RdfMediaType.Quads, _httpClient);
    }

    internal async Task UploadRdfData(string rdfData, RdfMediaType contentType)
    {
        if (contentType == RdfMediaType.JsonLd)
            ValidateJsonLd(rdfData);
        var request = new HttpRequestMessage(HttpMethod.Post, DataEndpointPath());
        request.Content = new StringContent(rdfData, contentType.GetMediaTypeHeaderValue());

        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to upload RDF data: {response.StatusCode} - {errorMessage}");
        }
    }

    internal SparqlQueryClient GetSparqlQueryClient() =>
        new SparqlQueryClient(_httpClient, SparqlEndpointUri());


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

        var request = new HttpRequestMessage(HttpMethod.Get, DataEndpointPath());
        request.Headers.Accept.Add(mediaType.GetMediaTypeWithQualityHeaderValue());
        var response = await _httpClient.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var errorMessage = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to retrieve RDF data: {response.StatusCode} - {errorMessage}");
        }
        var fusekiDatasetReponse = await response.Content.ReadAsStringAsync();
        if (mediaType == RdfMediaType.JsonLd)
            ValidateJsonLd(fusekiDatasetReponse);
        return fusekiDatasetReponse;
    }

    public override async Task<IEnumerable<INode>> SubjectWithType(UriNode type)
    {
        var queryString = $"SELECT ?x WHERE {{ GRAPH ?g {{ ?x a {type.ToString(new TurtleFormatter())} . }} }}";
        var queryClient = GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result => result.Value("x"));
    }

    public override async Task<IEnumerable<string>> LabelsOfSubject(UriNode subject)
    {
        string queryString = $"SELECT ?label WHERE {{ GRAPH ?g {{ {subject.ToString(new TurtleFormatter())} <http://www.w3.org/2000/01/rdf-schema#label> ?label . }} }}";
        var queryClient = GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            result.Value("label").ToString(new TurtleFormatter())
        );
    }



    public override async Task<IEnumerable<Triple>> TriplesWithSubject(UriNode subject)
    {
        string queryString = $"SELECT ?p ?o WHERE {{ GRAPH ?g {{ {subject.ToString(new TurtleFormatter())} ?p ?o . }} }}";
        var queryClient = GetSparqlQueryClient();
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
        var queryClient = GetSparqlQueryClient();
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
        var queryClient = GetSparqlQueryClient();
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
        var queryClient = GetSparqlQueryClient();
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
        var queryClient = GetSparqlQueryClient();
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
        string queryString = $"SELECT ?o WHERE {{ GRAPH ?g {{ {subject.ToString(turtleFormatter)} {predicate.ToString(turtleFormatter)} ?o . }} }}";
        var queryClient = GetSparqlQueryClient();
        var sparqlResultSet = await queryClient.QueryWithResultSetAsync(queryString);
        return sparqlResultSet.Select(result =>
            new Triple(subject,
                predicate,
                result.Value("o")
            ));
    }

    public override async Task<IGraph> ConstructQuery(SparqlQuery query)
    {
        var queryClient = GetSparqlQueryClient();
        return await queryClient.QueryWithResultGraphAsync(query.ToString());
    }

    public override async Task<SparqlResultSet> Query(SparqlQuery query)
    {
        var queryClient = GetSparqlQueryClient();
        return await queryClient.QueryWithResultSetAsync(query.ToString());
    }

    public override async Task<IEnumerable<string>> Sparql(string queryString)
    {
        var queryClient = GetSparqlQueryClient();
        var command = queryString.Split().First();
        return command.ToLower() switch
        {
            "construct" => (await queryClient.QueryWithResultGraphAsync(queryString))
                .Triples
                .Select(tr => tr.ToString(new TurtleFormatter())),
            "select" => (await queryClient.QueryWithResultSetAsync(queryString)).Results.Select(result =>
                result.ToString() ?? throw new InvalidOperationException("Null result from sparql query on record")),
            _ => throw new ArgumentException("Unsupported command in SPARQL query.")
        };
    }



    public override async Task<IGraph> GetMergedGraphs()
    {
        var queryClient = GetSparqlQueryClient();
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
        var queryClient = GetSparqlQueryClient();
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
        var queryClient = GetSparqlQueryClient();
        var qResult = await queryClient.QueryWithResultSetAsync(queryString);
        return qResult.Result;
    }

    public override async Task<string> ToCanonString()
    {
        var originalStore = await TripleStore();
        var canon = new RdfCanonicalizer().Canonicalize(originalStore);
        var canonStore = canon.OutputDataset;

        var stringWriter = new System.IO.StringWriter();
        var writer = new NQuadsWriter(NQuadsSyntax.Rdf11);
        writer.Save(canonStore, stringWriter);
        var result = stringWriter.ToString();
        return result;
    }
}