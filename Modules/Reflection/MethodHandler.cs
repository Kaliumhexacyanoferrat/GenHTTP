using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection;

/// <summary>
/// Allows to invoke a function on a service oriented resource.
/// </summary>
/// <remarks>
/// This provider analyzes the target method to be invoked and supplies
/// the required arguments. The result of the method is analyzed and
/// converted into a HTTP response.
/// </remarks>
public sealed class MethodHandler : IHandler
{
    private static readonly object?[] NoArguments = [];

    #region Initialization

    public MethodHandler(IHandler parent, Operation operation, Func<object> instanceProvider, IMethodConfiguration metaData,
        Func<IRequest, IHandler, Operation, object?, Action<IResponseBuilder>?, ValueTask<IResponse?>> responseProvider,
        MethodRegistry registry)
    {
        Parent = parent;

        Configuration = metaData;
        InstanceProvider = instanceProvider;

        ResponseProvider = responseProvider;

        Operation = operation;
        Registry = registry;
    }

    #endregion

    #region Get-/Setters

    public IHandler Parent { get; }

    public Operation Operation { get; }

    public IMethodConfiguration Configuration { get; }

    private Func<object> InstanceProvider { get; }

    private Func<IRequest, IHandler, Operation, object?, Action<IResponseBuilder>?, ValueTask<IResponse?>> ResponseProvider { get; }

    public MethodRegistry Registry { get; }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var arguments = await GetArguments(request);

        var result = Invoke(arguments);

        return await ResponseProvider(request, this, Operation, await UnwrapAsync(result), null);
    }

    private async ValueTask<object?[]> GetArguments(IRequest request)
    {
        var targetParameters = Operation.Method.GetParameters();

        Match? sourceParameters = null;

        if (!Operation.Path.IsIndex)
        {
            sourceParameters = Operation.Path.Matcher.Match(request.Target.GetRemaining().ToString());

            var matchedPath = WebPath.FromString(sourceParameters.Value);

            foreach (var _ in matchedPath.Parts) request.Target.Advance();
        }

        if (targetParameters.Length > 0)
        {
            var targetArguments = new object?[targetParameters.Length];

            var bodyArguments = FormFormat.GetContent(request);

            for (var i = 0; i < targetParameters.Length; i++)
            {
                var par = targetParameters[i];

                if (par.Name != null)
                {
                    if (Operation.Arguments.TryGetValue(par.Name, out var arg))
                    {
                        targetArguments[i] = arg.Source switch
                        {
                            OperationArgumentSource.Injected => ArgumentProvider.GetInjectedArgument(request, this, arg, Registry),
                            OperationArgumentSource.Path => ArgumentProvider.GetPathArgument(arg, sourceParameters, Registry),
                            OperationArgumentSource.Body => await ArgumentProvider.GetBodyArgumentAsync(request, arg, Registry),
                            OperationArgumentSource.Query => ArgumentProvider.GetQueryArgument(request, bodyArguments, arg, Registry),
                            OperationArgumentSource.Content => await ArgumentProvider.GetContentAsync(request, arg, Registry),
                            OperationArgumentSource.Streamed => ArgumentProvider.GetStream(request),
                            _ => throw new ProviderException(ResponseStatus.InternalServerError, $"Unable to map argument '{arg.Name}' of type '{arg.Type}' because source '{arg.Source}' is not supported")
                        };
                    }
                }
            }

            return targetArguments;
        }

        return NoArguments;
    }

    public ValueTask PrepareAsync() => ValueTask.CompletedTask;

    private object? Invoke(object?[] arguments)
    {
        try
        {
            return Operation.Method.Invoke(InstanceProvider(), arguments);
        }
        catch (TargetInvocationException e)
        {
            if (e.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(e.InnerException)
                                     .Throw();
            }

            throw;
        }
    }

    private static async ValueTask<object?> UnwrapAsync(object? result)
    {
        if (result == null)
        {
            return null;
        }

        var type = result.GetType();

        if (type.IsAsyncGeneric())
        {
            dynamic task = result;

            await task;

            return type.IsGenericallyVoid() ? null : task.Result;

        }
        if (type == typeof(ValueTask) || type == typeof(Task))
        {
            dynamic task = result;

            await task;

            return null;
        }

        return result;
    }

    #endregion

}
