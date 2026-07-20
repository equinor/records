namespace Records.RecordHandle;

/// <summary>
/// Fuseki-specific record handle used to reuse an existing dataset without reparsing RDF.
/// Assumes the receiver has access to the same Fuseki triplestore as the sender, and has read and write access to the dataset
/// </summary>
public sealed record RecordHandleV1
{
    public const string VersionV1 = "v1";
    public const string KindFusekiDatasetRef = "fuseki-dataset-ref";

    public string Dataset { get; init; }
    public string RecordId { get; init; }
    public DateTimeOffset ExpiresAt { get; init; }
    public string Kind { get; init; }
    public string Version { get; init; }

    public RecordHandleV1(
        string dataset,
        string recordId,
        DateTimeOffset expiresAt,
        string kind = KindFusekiDatasetRef,
        string version = VersionV1)
    {
        Dataset = dataset;
        RecordId = recordId;
        ExpiresAt = expiresAt;
        Kind = kind;
        Version = version;
    }

    public static RecordHandleV1 CreateFusekiDatasetRef(
        string dataset,
        string recordId,
        DateTimeOffset expiresAt)
    {
        return new RecordHandleV1(dataset, recordId, expiresAt);
    }

    public bool Verify(DateTimeOffset? now = null)
    {
        if (!string.Equals(Version, VersionV1, StringComparison.Ordinal)) return false;
        if (!string.Equals(Kind, KindFusekiDatasetRef, StringComparison.Ordinal)) return false;
        if (string.IsNullOrWhiteSpace(Dataset) || string.IsNullOrWhiteSpace(RecordId)) return false;

        var referenceNow = now ?? DateTimeOffset.UtcNow;
        if (referenceNow >= ExpiresAt) return false;

        return true;
    }
}
