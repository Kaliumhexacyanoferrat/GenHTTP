using GenHTTP.Api.Content;

namespace GenHTTP.Modules.Mcp.Logic;

public class ToolsHandlerBuilder : IHandlerBuilder<ToolsHandlerBuilder>
{
    private readonly List<IConcernBuilder> _Concerns = [];

    private readonly List<ITool> _Tools = [];

    public ToolsHandlerBuilder Add<TInput, TOutput>(ITool<TInput, TOutput> tool)
    {
        _Tools.Add(tool);
        return this;
    }

    public ToolsHandlerBuilder Add(IConcernBuilder concern)
    {
        _Concerns.Add(concern);
        return this;
    }

    public IHandler Build()
    {
        return Concerns.Chain(_Concerns, new ToolsHandler(_Tools));
    }

}
