using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.IO.Providers;

public sealed class InlineHandlerBuilder : IHandlerBuilder<InlineHandlerBuilder>
{
    private readonly List<IConcernBuilder> _concerns = [];
    
    private Func<IRequest, ValueTask<IResponse?>>? _logic;

    #region Functionality
    
    public InlineHandlerBuilder Logic(Func<IRequest, ValueTask<IResponse?>> logic)
    {
        _logic = logic;
        return this;
    }
    
    public InlineHandlerBuilder Add(IConcernBuilder concern)
    {
        _concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        if (_logic == null)
        {
            throw new BuilderMissingPropertyException("Logic");
        }
        
        return Concerns.Chain(_concerns, new InlineHandler(_logic));
    }
    
    #endregion

}
