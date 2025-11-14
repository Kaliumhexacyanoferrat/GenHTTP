using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers;

public sealed class InlineConcernBuilder : IConcernBuilder
{
    private Func<IRequest, IHandler, ValueTask<IResponse?>>? _logic;

    #region Functionality
    
    public InlineConcernBuilder Logic(Func<IRequest, IHandler, ValueTask<IResponse?>> logic)
    {
        _logic = logic;
        return this;
    }
    
    public IConcern Build(IHandler content)
    {
        if (_logic == null)
        {
            throw new BuilderMissingPropertyException("Logic");
        }
        
        return new InlineConcern(content, _logic);
    }
    
    #endregion
    
}
