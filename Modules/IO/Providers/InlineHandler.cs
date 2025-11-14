using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers;

public sealed class InlineHandler : IHandler
{
    private readonly Func<IRequest, ValueTask<IResponse?>> _logic;

    #region Initialization
    
    public InlineHandler(Func<IRequest, ValueTask<IResponse?>> logic)
    {
        _logic = logic;
    }
    
    #endregion
    
    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public ValueTask<IResponse?> HandleAsync(IRequest request) => _logic(request);

    #endregion
    
}
