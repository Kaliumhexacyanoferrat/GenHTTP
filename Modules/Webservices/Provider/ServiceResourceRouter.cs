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

    public IHandler Parent { get; }

    public ResponseProvider ResponseProvider { get; }

    public object Instance { get; }

    #endregion

    #region Initialization

    public ServiceResourceRouter(IHandler parent, object instance, MethodRegistry registry)
    {
        Parent = parent;

        Instance = instance;

        ResponseProvider = new ResponseProvider(registry);

        Methods = new MethodCollection(this, AnalyzeMethods(instance.GetType(), registry));
    }

    private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type, MethodRegistry registry)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

            if (attribute is not null)
            {
                var operation = OperationBuilder.Create(attribute.Path, method, registry);

                yield return parent => new MethodHandler(parent, operation, () => Instance, attribute, ResponseProvider.GetResponseAsync, registry);
            }
        }
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
