using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Modules.ErrorHandling.Mappers;

public class TextErrorMapper : IErrorMapper<Exception>
{
    // todo
    private readonly ReadOnlyMemory<byte> _internalServerError = "Internal Server Error"u8.ToArray();
    private readonly ReadOnlyMemory<byte> _notFound = "Not Found"u8.ToArray();

    public ValueTask<IResponse?> Map(IRequest request, IHandler handler, Exception error)
    {
        var response = GetStringResponse(request, error.ToString(), 500);

        return new(response);
    }

    public ValueTask<IResponse?> GetNotFound(IRequest request, IHandler handler)
    {
        var response = GetStringResponse(request, "Not Found", 404);

        return new(response);
    }

    private IResponse GetStringResponse(IRequest request, string text, int status)
    {
        return request.Respond()
                      .Raw()
                      .Status(status, status == 500 ? _internalServerError : _notFound)
                      .Content(new StringContent(text))
                      .Unraw()
                      .Build();
    }

}
