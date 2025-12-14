using System.Reflection;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Webservices.Provider;

public sealed class ServiceResourceRouter : IHandler, IServiceMethodProvider
{

    #region Get-/Setters

    private Type Type { get; }

    private Func<IRequest, ValueTask<object>> InstanceProvider { get; }

    private MethodRegistry Registry { get; }

    private ExecutionMode? ExecutionMode { get; }

    public MethodCollectionFactory Methods { get; }

    #endregion

    #region Initialization

    public ServiceResourceRouter(Type type, Func<IRequest, ValueTask<object>> instanceProvider, ExecutionMode? mode, MethodRegistry registry)
    {
        Type = type;
        InstanceProvider = instanceProvider;
        ExecutionMode = mode;
        Registry = registry;

        Methods = new MethodCollectionFactory(GetMethodsAsync);
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    public async ValueTask<IResponse?> HandleAsync(IRequest request) => await (await Methods.GetAsync(request)).HandleAsync(request);

    private async Task<MethodCollection> GetMethodsAsync(IRequest request)
    {
        var found = new List<MethodHandler>();

        foreach (var method in Type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
        {
            var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

            if (attribute is not null)
            {
                var operation = OperationBuilder.Create(request, attribute.Path, method, null, ExecutionMode, Registry);

                found.Add(new MethodHandler(operation, InstanceProvider, attribute, Registry));
            }
        }

        var result = new MethodCollection(found);

        await result.PrepareAsync();

        return result;
    }

    #endregion

}
