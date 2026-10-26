using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Authentication;
using GenHTTP.Api.Content.Services;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Authentication.ApiKey;

public sealed class ApiKeyConcern : IDocumentedConcern
{

    #region Get-/Setters

    public IHandler Content { get; }

    private Func<IRequest, string?> KeyExtractor { get; }

    private Func<IRequest, string, ValueTask<IUser?>> Authenticator { get; }

    private SecurityScheme? Scheme { get; }

    #endregion

    #region Initialization

    public ApiKeyConcern(IHandler content, Func<IRequest, string?> keyExtractor, Func<IRequest, string, ValueTask<IUser?>> authenticator, SecurityScheme? scheme = null)
    {
        Content = content;

        KeyExtractor = keyExtractor;
        Authenticator = authenticator;
        Scheme = scheme;
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var key = KeyExtractor(request);

        if (key != null)
        {
            var user = await Authenticator(request, key);

            if (user != null)
            {
                request.SetUser(user);

                return await Content.HandleAsync(request);
            }
            return request.Respond()
                          .Status(ResponseStatus.Forbidden)
                          .Build();
        }

        return request.Respond()
                      .Status(ResponseStatus.Unauthorized)
                      .Build();
    }

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public void AddDocumentation(OperationDocumentation operation)
    {
        if (Scheme != null)
        {
            operation.RequireAuthentication(Scheme);
        }

        operation.Response(ResponseStatus.Unauthorized, "No API key has been sent with the request")
                 .Response(ResponseStatus.Forbidden, "The API key sent with the request is not valid");
    }

    #endregion

}
