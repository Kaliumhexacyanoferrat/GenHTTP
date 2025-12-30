using System.Runtime.CompilerServices;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection.Routing;

namespace GenHTTP.Modules.Reflection;

public sealed class MethodCollection : IHandler
{

    #region Get-/Setters

    public List<MethodHandler> Methods { get; }

    #endregion

    #region Initialization

    public MethodCollection(IEnumerable<MethodHandler> methods)
    {
        Methods = [..methods];
    }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        bool foundOthers = false;
        
        (MethodHandler, RoutingMatch)? directMatch = null;
        (MethodHandler, RoutingMatch)? wildcardMatch = null;

        var requestTarget = request.Target;
        
        for (var i = 0; i < Methods.Count; i++)
        {
            var method = Methods[i];

            var operation = method.Operation;
            
            var route = operation.Route;
            
            var match = OperationRouter.TryMatch(requestTarget, route);

            if (match != null)
            {
                if (!operation.Configuration.SupportedMethods.Contains(request.Method))
                {
                    foundOthers = true;
                    continue;
                }
                
                if (route.IsWildcard)
                {
                    wildcardMatch = (method, match);
                }
                else if (directMatch != null)
                {
                    throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{requestTarget.Path}'");
                }
                else
                {
                    directMatch = (method, match);
                }
            }
        }

        if (directMatch != null)
        {
            return RegisterMatchAndExecute(request, directMatch.Value);
        }

        if (wildcardMatch != null)
        {
            return RegisterMatchAndExecute(request, wildcardMatch.Value);
        }

        if (foundOthers)
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "There is no method of a matching request type");
        }

        return new ValueTask<IResponse?>();
    }

    public async ValueTask PrepareAsync()
    {
        foreach (var handler in Methods)
        {
            await handler.PrepareAsync();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private ValueTask<IResponse?> RegisterMatchAndExecute(IRequest request, (MethodHandler, RoutingMatch) match)
    {
        request.Properties[MethodHandler.MatchProperty] = match.Item2;
        return match.Item1.HandleAsync(request);
    }

    #endregion

}
