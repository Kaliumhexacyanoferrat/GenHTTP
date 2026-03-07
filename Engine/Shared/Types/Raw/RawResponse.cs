using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types;

public class RawResponse : IRawResponse
{
    private static readonly ReadOnlyMemory<byte> NoContent = "No Content"u8.ToArray();

    private readonly EditableKeyValueList _headers = new();

    public int StatusCode { get; set; }

    public ReadOnlyMemory<byte> StatusPhrase { get; set; }

    public EditableKeyValueList EditableHeaders  => _headers;

    public IRawKeyValueList Headers => _headers;

    public IResponseContent? Content { get; set; }

    public void Reset()
    {
        StatusCode = 204;
        StatusPhrase = NoContent;

        Content = null;

        _headers.Clear();
    }

}
