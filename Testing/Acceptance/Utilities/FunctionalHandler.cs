using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities;

public sealed class FunctionalHandler : IHandler
{
    private readonly Func<IRequest, IResponse?>? _responseProvider;

    #region Initialization

    public FunctionalHandler(Func<IRequest, IResponse?>? responseProvider = null)
    {
        _responseProvider = responseProvider;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync(IServer server) => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => new(_responseProvider?.Invoke(request));

    #endregion

}
