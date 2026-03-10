using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Modules.ErrorHandling.Mappers;

public class TextErrorMapper : IErrorMapper<Exception>
{

    public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        var response = GetStringResponse(request, error.ToString(), ResponseStatus.InternalServerError);

        return new(response);
    }

    public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        var response = GetStringResponse(request, "Not Found", ResponseStatus.BadRequest);

        return new(response);
    }

    private IResponse GetStringResponse(IRequest request, string text, ResponseStatus status)
    {
        return request.Respond()
                      .Raw()
                      .Status(status)
                      .Content(new StringContent(text))
                      .Unraw()
                      .Build();
    }

}
