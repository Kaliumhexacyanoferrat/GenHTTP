using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Authentication;
using StringContent = GenHTTP.Modules.IO.Strings.StringContent;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

public class UserReturningHandler : IHandler
{

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var content = request.GetUser<IUser>()?.DisplayName ?? throw new ProviderException(ResponseStatus.BadRequest, "No user!");

        return request.Respond()
                      .Content(new StringContent(content))
                      .BuildTask();
    }

}
