using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types.Raw;

public class RawResponseBuilder(Response response, ResponseBuilder builder) : IRawResponseBuilder
{

    public IRawResponseBuilder Status(ResponseStatus code)
    {
        response.Source.Status = code;
        return this;
    }

    public IRawResponseBuilder Connection(Connection mode)
    {
        response.Source.Mode = mode;
        return this;
    }

    public IRawResponseBuilder Header(ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value)
    {
        response.Source.EditableHeaders.Add(name, value);
        return this;
    }

    public IRawResponseBuilder Content(IResponseContent? content)
    {
        if (content != null)
        {
            // todo?
            if (response.Source.Status == ResponseStatus.NoContent)
            {
                response.Source.Status = ResponseStatus.Ok;
            }
        }

        response.Source.Content = content;
        return this;
    }

    public IResponseBuilder ToHighLevel() => builder;

    public IResponse Build() => response;

}
