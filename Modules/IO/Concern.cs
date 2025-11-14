using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO.Providers;

namespace GenHTTP.Modules.IO;

public static class Concern
{

    /// <summary>
    /// Creates a concern from the given logic that can be added to
    /// other handlers as needed.
    /// </summary>
    /// <param name="logic">The logic to be executed</param>
    /// <returns>The newly created builder</returns>
    public static InlineConcernBuilder From(Func<IRequest, IHandler, ValueTask<IResponse?>> logic)
        => new InlineConcernBuilder().Logic(logic);
    
}
