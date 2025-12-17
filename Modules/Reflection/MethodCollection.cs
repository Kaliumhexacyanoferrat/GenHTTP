using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

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
        var methods = FindProviders(request.Target.GetRemaining().ToString(), request.Method, out var others);

        if (methods.Count == 1)
        {
            return methods[0].HandleAsync(request);
        }
        if (methods.Count > 1)
        {
            // if there is only one non-wildcard, use this one
            var nonWildcards = methods.Where(m => !m.Operation.Path.IsWildcard).ToList();

            if (nonWildcards.Count == 1)
            {
                return nonWildcards[0].HandleAsync(request);
            }

            throw new ProviderException(ResponseStatus.BadRequest, $"There are multiple methods matching '{request.Target.Path}'");
        }

        if (others.Count > 0)
        {
            throw new ProviderException(ResponseStatus.MethodNotAllowed, "There is no method of a matching request type", AddAllowHeader);

            void AddAllowHeader(IResponseBuilder b)
            {
                b.Header("Allow", string.Join(", ", others.Select(o => o.RawMethod.ToUpper())));
            }
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

    private List<MethodHandler> FindProviders(string path, FlexibleRequestMethod requestedMethod, out HashSet<FlexibleRequestMethod> otherMethods)
    {
        otherMethods = [];

        var result = new List<MethodHandler>(2);

        foreach (var method in Methods)
        {
            var operation = method.Operation;

            var configuration = operation.Configuration;

            if (method.Operation.Path.IsIndex && path == "/")
            {
                if (method.Operation.Configuration.SupportedMethods.Contains(requestedMethod))
                {
                    result.Add(method);
                }
                else
                {
                    otherMethods.UnionWith(configuration.SupportedMethods);
                }
            }
            else
            {
                if (method.Operation.Path.Matcher.IsMatch(path))
                {
                    if (configuration.SupportedMethods.Contains(requestedMethod))
                    {
                        result.Add(method);
                    }
                    else
                    {
                        otherMethods.UnionWith(configuration.SupportedMethods);
                    }
                }
            }
        }

        return result;
    }

    #endregion

}
