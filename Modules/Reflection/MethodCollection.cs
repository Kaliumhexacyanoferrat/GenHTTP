using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Reflection;

public sealed class MethodCollection : IHandler
{

    #region Initialization

    public MethodCollection(IHandler parent, IEnumerable<Func<IHandler, MethodHandler>> methodFactories)
    {
        Parent = parent;

        Methods = methodFactories.Select(factory => factory(this))
                                 .ToList();
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent { get; }

    public List<MethodHandler> Methods { get; }

    #endregion

    #region Functionality

    public ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var methods = FindProviders(request.Target.GetRemaining().ToString(), request.Method, out var foundOthers);

        if (methods.Count == 1)
        {
            return methods[0].HandleAsync(request);
        }
        if (methods.Count > 1)
        {
            // if there is only one non-wildcard, use this one
            var nonWildcards = methods.Where(m => !m.Routing.IsWildcard).ToList();

            if (nonWildcards.Count == 1)
            {
                return nonWildcards[0].HandleAsync(request);
            }

            throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{request.Target.Path}'");
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

    private List<MethodHandler> FindProviders(string path, FlexibleRequestMethod requestedMethod, out bool foundOthers)
    {
        foundOthers = false;

        var result = new List<MethodHandler>(2);

        foreach (var method in Methods)
        {
            if (method.Routing.IsIndex && path == "/")
            {
                if (method.Configuration.SupportedMethods.Contains(requestedMethod))
                {
                    result.Add(method);
                }
                else
                {
                    foundOthers = true;
                }
            }
            else
            {
                if (method.Routing.ParsedPath.IsMatch(path))
                {
                    if (method.Configuration.SupportedMethods.Contains(requestedMethod))
                    {
                        result.Add(method);
                    }
                    else
                    {
                        foundOthers = true;
                    }
                }
            }
        }

        return result;
    }

    #endregion

}
