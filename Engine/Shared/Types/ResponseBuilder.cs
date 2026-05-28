using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Engine.Shared.Types;

public class ResponseBuilder : IResponseBuilder
{
    private readonly Response _response;

    public ResponseBuilder()
    {
        _response = new(this);
    }

    public IResponseBuilder Status(ResponseStatus status)
    {
        _response.Status = status;
        return this;
    }

    public IResponseBuilder Connection(Connection mode)
    {
        _response.Mode = mode;
        return this;
    }

    public IResponseBuilder Header(ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value)
    {
        _response.EditableHeaders.Add(name, value);
        return this;
    }
    
    public IResponseBuilder Header(string name, string value)
        => Header(Encoding.ASCII.GetBytes(name), Encoding.ASCII.GetBytes(value));
    
    public IResponseBuilder Content(IResponseContent? content)
    {
        if (content != null)
        {
            if (_response.Status == ResponseStatus.NoContent)
            {
                _response.Status = ResponseStatus.Ok;
            }
        }

        _response.Content = content;
        return this;
    }

    public IResponse Build() => _response;

    public void Reset() => _response.Reset();

}
