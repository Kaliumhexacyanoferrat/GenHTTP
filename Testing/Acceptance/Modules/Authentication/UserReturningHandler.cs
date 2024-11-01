using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Authentication;
using GenHTTP.Modules.IO;

namespace GenHTTP.Testing.Acceptance.Modules.Authentication;

public class UserReturningHandler : IHandler
{

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var content = request.GetUser<IUser>()?.DisplayName ?? throw new ProviderException(ResponseStatus.BadRequest, "No user!");

        return request.Respond()
                      .Content(content)
                      .BuildTask();
    }

}
