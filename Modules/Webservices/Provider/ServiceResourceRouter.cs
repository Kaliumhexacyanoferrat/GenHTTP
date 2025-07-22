using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Webservices.Provider;

public sealed class ServiceResourceRouter : IHandler, IServiceMethodProvider
{

    #region Get-/Setters

    public MethodCollection Methods { get; }

    #endregion

    #region Initialization

    public ServiceResourceRouter(Type type, Func<IRequest, ValueTask<object>> instanceProvider, MethodRegistry registry)
    {
        Methods = new MethodCollection(AnalyzeMethods(type, instanceProvider, registry));
    }

    private IEnumerable<MethodHandler> AnalyzeMethods(Type type, Func<IRequest, ValueTask<object>> instanceProvider, MethodRegistry registry)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

            if (attribute is not null)
            {
                var operation = OperationBuilder.Create(attribute.Path, method, registry);

                yield return new MethodHandler(operation, instanceProvider, attribute, registry);
            }
        }
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
