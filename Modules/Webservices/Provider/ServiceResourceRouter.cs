using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Serializers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Webservices.Provider;

public sealed class ServiceResourceRouter : IHandler
{

    #region Get-/Setters

    private MethodCollection Methods { get; }

    public IHandler Parent { get; }

    public ResponseProvider ResponseProvider { get; }

    public object Instance { get; }

    #endregion

    #region Initialization

    public ServiceResourceRouter(IHandler parent, object instance, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
    {
        Parent = parent;

        Instance = instance;

        ResponseProvider = new ResponseProvider(serialization, formatting);

        Methods = new MethodCollection(this, AnalyzeMethods(instance.GetType(), serialization, injection, formatting));
    }

    private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type, SerializationRegistry serialization, InjectionRegistry injection, FormatterRegistry formatting)
    {
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

            if (attribute is not null)
            {
                var operation = OperationBuilder.Create(attribute.Path, method);

                yield return parent => new MethodHandler(parent, method, operation, () => Instance, attribute, ResponseProvider.GetResponseAsync, serialization, injection, formatting);
            }
        }
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => Methods.PrepareAsync();

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
