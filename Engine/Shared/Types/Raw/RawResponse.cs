using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RawResponse : IRawResponse
{
    private readonly EditableKeyValueList _headers = new();

    public ResponseStatus Status { get; set; }

    public Connection Mode { get; set; }

    public EditableKeyValueList EditableHeaders => _headers;

    public IRawKeyValueList Headers => _headers;

    public IResponseContent? Content { get; set; }

    public RawResponse()
    {
        Status = ResponseStatus.NoContent;
        Mode = Connection.KeepAlive;
    }

    public void Reset()
    {
        if (Status != ResponseStatus.NoContent)
        {
            Status = ResponseStatus.NoContent;
        }

        if (Mode != Connection.KeepAlive)
        {
            Mode = Connection.KeepAlive;
        }

        if (Content != null)
        {
            Content = null;
        }

        _headers.Clear();
    }

}
