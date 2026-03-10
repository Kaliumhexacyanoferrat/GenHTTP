using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RawResponse : IRawResponse
{
    private readonly EditableKeyValueList _headers = new();

    public ResponseStatus Status { get; set; }

    public EditableKeyValueList EditableHeaders  => _headers;

    public IRawKeyValueList Headers => _headers;

    public IResponseContent? Content { get; set; }

    public void Reset()
    {
        Status = ResponseStatus.NoContent;

        Content = null;

        _headers.Clear();
    }

}
