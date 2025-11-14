using GenHTTP.Api.Protocol;
using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO;

public static class Handler
{
    
    /// <summary>
    /// Creates a handler from the given logic.
    /// </summary>
    /// <param name="logic">The logic to be executed</param>
    /// <returns>The newly created builder</returns>
    public static InlineHandlerBuilder From(Func<IRequest, ValueTask<IResponse?>> logic)
        => new InlineHandlerBuilder().Logic(logic);

    /// <summary>
    /// Creates a handler from the given logic.
    /// </summary>
    /// <param name="logic">The logic to be executed</param>
    /// <returns>The newly created builder</returns>
    public static InlineHandlerBuilder From(Func<IRequest, IResponse?> logic)
        => From((r) => new ValueTask<IResponse?>(logic(r)));

}
