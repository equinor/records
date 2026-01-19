using IriTools;
using Microsoft.AspNetCore.WebUtilities;
using Records.Backend;

namespace Records.Sender;

public record RecordMessageBuilder
{
    private string? _token;
    private Uri? _endpoint;
    private IriReference? _recordId;
    private string? _cursor;
    private Immutable.Record? _record;

    public RecordMessageBuilder() { }

    public RecordMessageBuilder(string token) => _token = token;

    public RecordMessageBuilder WithToken(string token) =>
        this with
        {
            _token = token
        };


    public RecordMessageBuilder ForEndpoint(Uri endpoint) =>
        this with
        {
            _endpoint = endpoint
        };

    public RecordMessageBuilder ForEndpoint(string endpoint) => ForEndpoint(new Uri(endpoint));

    public RecordMessageBuilder WithRecord(Immutable.Record record) =>
        this with
        {
            _record = record,
            _recordId = record.Id
        };

    public RecordMessageBuilder WithRecord(string record) => WithRecord(new Immutable.Record(new DotNetRdfRecordBackend(record)));

    public RecordMessageBuilder WithRecordId(string recordId) =>
    this with
    {
        _recordId = recordId
    };

    public RecordMessageBuilder WithRecordId(Immutable.Record record) => WithRecord(record);

    public RecordMessageBuilder WithCursor(string cursor) =>
        this with
        {
            _cursor = cursor
        };

    public RecordMessageBuilder WithCursor(int cursor) => WithCursor(cursor.ToString());

    public HttpRequestMessage Build(RecordOperation operation)
    {
        return operation switch
        {
            RecordOperation.Send => BuildSender(),
            RecordOperation.Retract => BuildRetracter(),
            _ => throw new Exception("You need to select an operation.")
        };
    }

    private HttpRequestMessage BuildSender()
    {
        ArgumentNullException.ThrowIfNull(_token);
        ArgumentNullException.ThrowIfNull(_endpoint);
        ArgumentNullException.ThrowIfNull(_record);
        ArgumentNullException.ThrowIfNull(_cursor);

        var endpointUri = QueryHelpers.AddQueryString(_endpoint.ToString(), "cursor", _cursor);

        var message = new HttpRequestMessage
        {
            RequestUri = new Uri(endpointUri),
            Method = HttpMethod.Post,
            Headers =
                {
                    { "Authorization", $"Bearer {_token}" }
                }
        };

        message.AddRecord(_record);

        return message;
    }

    private HttpRequestMessage BuildRetracter()
    {
        ArgumentNullException.ThrowIfNull(_token);
        ArgumentNullException.ThrowIfNull(_endpoint);
        ArgumentNullException.ThrowIfNull(_recordId);

        var message = new HttpRequestMessage
        {
            RequestUri = _endpoint,
            Method = HttpMethod.Delete,
            Headers =
            {
                    { "Authorization", $"Bearer {_token}" }
                }
        };

        message.AddRecordId(_recordId);

        return message;
    }

    public enum RecordOperation
    {
        None = 0,
        Send = 1,
        Retract = 2
    }
}
