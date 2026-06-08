using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers;

public sealed class InlineConcern : IConcern
{
    private readonly Func<IRequest, IHandler, ValueTask<IResponse?>> _logic;

    #region Get-/Setters
    
    public IHandler Content { get; }
    
    #endregion
    
    #region Initialization
    
    public InlineConcern(IHandler content, Func<IRequest, IHandler, ValueTask<IResponse?>> logic)
    {
        Content = content;
        
        _logic = logic;
    }
    
    #endregion
    
    #region Functionality

    public ValueTask PrepareAsync(IServer server) => Content.PrepareAsync(server);

    public ValueTask<IResponse?> HandleAsync(IRequest request) => _logic(request, Content);

    #endregion
    
}
