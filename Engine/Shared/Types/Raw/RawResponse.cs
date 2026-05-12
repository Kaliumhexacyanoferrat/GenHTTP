using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RawResponse : IRawResponse
{
    private readonly EditableKeyValueList _headers = new();

    public ResponseStatus Status { get; set; }

    public Connection Mode { get; set; }

    public EditableKeyValueList EditableHeaders  => _headers;

    public IRawKeyValueList Headers => _headers;

    public IResponseContent? Content { get; set; }

    public void Reset()
    {
        Status = ResponseStatus.NoContent;
        Mode = Connection.KeepAlive;

        Content = null;

        _headers.Clear();
    }

}
