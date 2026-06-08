using System.Reflection;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Webservices.Provider;

public sealed class ServiceResourceRouter : IHandler, IServiceMethodProvider
{
    private MethodCollection? _methods;

    #region Get-/Setters

    private Type Type { get; }

    private Func<IRequest, ValueTask<object>> InstanceProvider { get; }

    private MethodRegistry Registry { get; }

    private ExecutionSettings ExecutionSettings { get; }

    public MethodCollection Methods => _methods ?? throw new InvalidOperationException("Handler is not prepared yet");

    #endregion

    #region Initialization

    public ServiceResourceRouter(Type type, Func<IRequest, ValueTask<object>> instanceProvider, ExecutionSettings executionSettings, MethodRegistry registry)
    {
        Type = type;
        InstanceProvider = instanceProvider;
        ExecutionSettings = executionSettings;
        Registry = registry;
    }

    #endregion

    #region Functionality

    public async ValueTask PrepareAsync(IServer server)
    {
        var found = new List<MethodHandler>();

        foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

            if (attribute is not null)
            {
                var operation = OperationBuilder.Create(server, attribute.Path, method, null, ExecutionSettings, attribute, Registry);

                found.Add(new MethodHandler(operation, InstanceProvider, Registry));
            }
        }

        var result = new MethodCollection(found);

        await result.PrepareAsync(server);

        _methods = result;
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

    #endregion

}
