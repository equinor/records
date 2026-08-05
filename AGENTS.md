# Records Repository - Agent Knowledge Base

This document captures repository-specific insights useful for AI agents working on the Records library.

## Architecture & Design Philosophy

### Record Immutability Model
- **Content is immutable**: RDF triples describing the entity cannot be changed
- **Metadata is mutable**: Provenance, relationships, replaces, describes links are additive
- This distinction is reflected in method naming: `WithAdditionalMetadata` implies "add to", not "replace"
- The `WithAdditionalMetadata` method mutates the underlying Fuseki dataset in-place

### Backend Architecture
- All backends inherit from `RecordBackendBase` (handles metadata caching via `MetadataGraph` property)
- Implementations: `FusekiRecordBackend`, `DotNetRdfRecordBackend`
- Records are wrapped by `Immutable.Record` which delegates to backend

## Critical Implementation Details

### Metadata Caching (Important!)
- `RecordBackendBase` caches `MetadataGraph` after calling `InitializeMetadata()`
- **When mutating metadata in-place, you MUST call `InitializeMetadata()` after the mutation**
- Without refresh, newly added triples won't be visible in subsequent queries
- This was the critical fix for the WithAdditionalMetadata OOM issue

### Fuseki Graph Operations
- **Format matters**: Use `NTriples` for single-graph POSTs (via `AddGraphAsync`), `NQuads` for multi-graph
- **Graph endpoint**: `{datasetName}/data?graph={uri}` for graph-specific operations
- **Data endpoint**: `{datasetName}/data` for general RDF operations
- Posting to wrong format = parse errors (experienced with attempting to embed NQuads in SPARQL)

### Streaming Large Records
- Use `ToStream(RdfMediaType.Quads)` to stream record content without buffering to memory
- Wrap in `StreamContent` for HTTP POST
- Critical for avoiding OOM with records > heap size
- Never call `ReadAsStringAsync()` on large responses from Fuseki

### Dataset Lifecycle
- Create with `CreateDatasetAsync()` (uses `BuildDatasetAssembler()`)
- Ensure exists with `EnsureDatasetExistsAsync()`
- Delete with `DeleteDatasetAsync()`
- Always wrap creation in try/catch and clean up on failure

## Key Fuseki Endpoints

| Endpoint | Purpose | Format |
|----------|---------|--------|
| `{dataset}/data` | General RDF operations | Configurable (N-Triples, N-Quads, JSON-LD) |
| `{dataset}/data?graph={uri}` | Graph-specific operations | N-Triples |
| `{dataset}/update` | SPARQL UPDATE | application/sparql-update |
| `{dataset}/sparql` | SPARQL QUERY | application/sparql-results+json |
| `{dataset}/shacl` | SHACL validation | Configurable |

## Testing Infrastructure

### Test Setup
- Fuseki runs in Docker container via `testcontainers-dotnet`
- DotNetRdf backend for in-memory/fast tests
- Test data located in `TestData` class

### Test Patterns
- Always test both `BackendType.DotNetRdf` (fast) and `BackendType.Fuseki` (integration)
- DotNetRdf tests run instantly; Fuseki tests spin up Docker (~3-5s overhead)
- All tests currently pass (130 total)

## Common Patterns & Code Reuse

### When Adding Metadata
```csharp
var metadataGraph = new Graph(RecordId);
metadataGraph.Assert(additionalMetadata.Triples);
await AddGraphAsync(metadataGraph);
await InitializeMetadata(); // CRITICAL: refresh cache
return this;
```

### When Streaming Large Content
```csharp
await using var sourceStream = await ToStream(RdfMediaType.Quads);
var content = new StreamContent(sourceStream);
content.Headers.ContentType = RdfMediaType.Quads.GetMediaTypeHeaderValue();
// POST content to Fuseki
```

### When Creating New Datasets
```csharp
var newDatasetName = $"record_{Guid.NewGuid()}";
var newBackend = new FusekiRecordBackend(_httpClient, newDatasetName);
try
{
    await newBackend.CreateDatasetAsync();
    // ... populate dataset ...
    await newBackend.InitializeMetadata();
    return newBackend;
}
catch
{
    await newBackend.DeleteDatasetAsync(); // cleanup
    throw;
}
```

## Performance Considerations

### OOM Prevention
- **DO**: Stream large records via `ToStream()`
- **DON'T**: Call `GetRdfDataAsString()` on records > heap size
- **DO**: Mutate metadata in-place when possible (vs creating new datasets)
- **DON'T**: Buffer entire Fuseki responses as strings

### Query Efficiency
- Use SPARQL queries for complex pattern matching
- `TriplesWithPredicates()` is efficient for filtering by predicate URIs
- Construct queries are more efficient than SELECT for graph extraction

## Build & Test Commands

```bash
# Build from Records repo root
cd src/Record
dotnet build

# Run all tests
dotnet test --verbosity minimal

# Run specific test
dotnet test --filter "Record_Can_Add_Metadata"

# Run without Docker/Fuseki (tests that need containers will skip)
dotnet test --filter "BackendType=DotNetRdf"
```

## Dependencies & Versions

- **VDS.RDF**: RDF parsing/serialization (NTriples, NQuads, JSON-LD, Turtle)
- **dotnet 10.0**: Target framework
- **testcontainers-dotnet**: For Fuseki Docker integration
- **xunit**: Testing framework

## Common Gotchas

1. **Metadata not updated**: Forgot to call `InitializeMetadata()` after mutation
2. **Parse errors with SPARQL**: Tried to embed NQuads/NTriples directly in SPARQL syntax (use POST to endpoint instead)
3. **OOM on large records**: Called `ReadAsStringAsync()` instead of streaming with `ToStream()`
4. **Stale cache**: Modified backend state without refreshing cached properties
5. **Missing graph name**: Graphs MUST have a URI name (base URI) when added to TripleStore

## Documentation & Specs

- SPARQL spec: https://www.w3.org/TR/sparql11-query/
- Turtle/N-Triples: https://www.w3.org/TR/turtle/
- JSON-LD: https://www.w3.org/TR/json-ld11/
- Fuseki API: https://jena.apache.org/documentation/fuseki2/

## Recent Insights from Fixes

### WithAdditionalMetadata Refactor (AB#487397)
- **Problem**: Buffering entire record as string caused OOM
- **Solution**: Use AddGraphAsync + metadata refresh instead of recreating datasets
- **Key learning**: Metadata mutation (in-place) is intentional per API design; not a bug
- **Testing**: Both DotNetRdf and Fuseki backends must pass (content behavior differs)

