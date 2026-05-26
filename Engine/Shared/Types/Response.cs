using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class Response : IResponse
{
    private readonly EditableKeyValueList _headers = new();

    private readonly IResponseBuilder _builder;
    
    public ResponseStatus Status { get; set; }

    public Connection Mode { get; set; }
    
    public IResponseContent? Content { get; set; }

    public IKeyValueList Headers => _headers;

    public EditableKeyValueList EditableHeaders => _headers;
    
    public Response(IResponseBuilder builder)
    {
        Status = ResponseStatus.NoContent;
        Mode = Connection.KeepAlive;
        
        _builder = builder;
    }
    
    public IResponseBuilder Rebuild() => _builder;
    
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
