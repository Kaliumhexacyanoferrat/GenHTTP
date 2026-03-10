using GenHTTP.Api.Protocol;
using GenHTTP.Api.Protocol.Raw;

namespace GenHTTP.Engine.Shared.Types;

public class RawResponseBuilder(Response response, ResponseBuilder builder) : IRawResponseBuilder
{

    public IRawResponseBuilder Status(ResponseStatus code)
    {
        response.Source.Status = code;
        return this;
    }


    public IRawResponseBuilder Header(ReadOnlyMemory<byte> name, ReadOnlyMemory<byte> value)
    {
        response.Source.EditableHeaders.Add(name, value);
        return this;
    }

    public IRawResponseBuilder Content(IResponseContent? content)
    {
        response.Source.Content = content;
        return this;
    }

    public IResponseBuilder Unraw() => builder;

    public IResponse Build() => response;

}
