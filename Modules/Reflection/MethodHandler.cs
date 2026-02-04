using System.Reflection;
using System.Runtime.ExceptionServices;

using Cottle;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Serializers.Forms;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Pages;
using GenHTTP.Modules.Pages.Rendering;
using GenHTTP.Modules.Reflection.Generation;
using GenHTTP.Modules.Reflection.Operations;
using GenHTTP.Modules.Reflection.Routing;

namespace GenHTTP.Modules.Reflection;

public delegate ValueTask<IResponse?> RequestInterception(IRequest request, IReadOnlyDictionary<string, object?> arguments);

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
    private static readonly Dictionary<string, object?> NoArguments = [];
    
    private static readonly TemplateRenderer ErrorRenderer = Renderer.From(Resource.FromAssembly("CodeGenerationError.html").Build());

    private Func<object, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>? _compiledMethod;

    private Func<Delegate, Operation, IRequest, IHandler, MethodRegistry, RoutingMatch, RequestInterception, ValueTask<IResponse?>>? _compiledDelegate;

    private CodeGenerationException? _compilationError;

    private readonly RequestInterception _interceptor;

    #region Get-/Setters

    public Operation Operation { get; }

    private Func<IRequest, ValueTask<object>> InstanceProvider { get; }

    public MethodRegistry Registry { get; }

    private ResponseProvider ResponseProvider { get; }

    private bool UseCodeGeneration { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new handler to serve a single API operation.
    /// </summary>
    /// <param name="operation">The operation to be executed and provided (use <see cref="OperationBuilder"/> to create an operation)</param>
    /// <param name="instanceProvider">A factory that will provide an instance to actually execute the operation on</param>
    /// <param name="registry">The customized registry to be used to read and write data</param>
    public MethodHandler(Operation operation, Func<IRequest, ValueTask<object>> instanceProvider, MethodRegistry registry)
    {
        InstanceProvider = instanceProvider;

        Operation = operation;
        Registry = registry;

        ResponseProvider = new(registry);

        var effectiveMode = Operation.ExecutionSettings.Mode ?? ExecutionMode.Reflection;

        UseCodeGeneration = OptimizedDelegate.Supported && effectiveMode == ExecutionMode.Auto;
        
        _interceptor = InterceptAsync;
    }

    #endregion

    #region Functionality

    public ValueTask PrepareAsync()
    {
        if (UseCodeGeneration)
        {
            try
            {
                if (Operation.Delegate != null)
                {
                    _compiledDelegate = OptimizedDelegate.Compile<Delegate>(Operation);
                }
                else
                {
                    _compiledMethod = OptimizedDelegate.Compile<object>(Operation);
                }
            }
            catch (CodeGenerationException e)
            {
                _compilationError = e;
            }
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask<IResponse?> HandleAsync(IRequest request) => HandleAsync(request, new(0, null));

    public ValueTask<IResponse?> HandleAsync(IRequest request, RoutingMatch match)
    {
        if (match.Offset > 0)
        {
            request.Target.Advance(match.Offset);
        }
        
        if (UseCodeGeneration)
        {
            if (_compilationError != null)
            {
                return RenderCompilationErrorAsync(request, _compilationError);
            }

            return Operation.Delegate != null ? RunAsDelegate(request, match) : RunAsMethod(request, match);
        }

        return RunViaReflection(request, match);
    }

    private ValueTask<IResponse?> RunAsDelegate(IRequest request, RoutingMatch match)
    {
        if (_compiledDelegate == null || Operation.Delegate == null)
            throw new InvalidOperationException("Compiled delegate is not initialized");

        return _compiledDelegate(Operation.Delegate, Operation, request, this, Registry, match, _interceptor);
    }

    private async ValueTask<IResponse?> RunAsMethod(IRequest request, RoutingMatch match)
    {
        if (_compiledMethod == null)
            throw new InvalidOperationException("Compiled method is not initialized");

        var instance = await InstanceProvider(request);

        return await _compiledMethod(instance, Operation, request, this, Registry, match, _interceptor);
    }

    private async ValueTask<IResponse?> RunViaReflection(IRequest request, RoutingMatch match)
    {
        var arguments = await GetArguments(request, match);

        var interception = await InterceptAsync(request, arguments);

        if (interception is not null)
        {
            return interception;
        }

        var result = await InvokeAsync(request, arguments.Values.ToArray());

        return await ResponseProvider.GetResponseAsync(request, Operation, await UnwrapAsync(result));
    }

    private async ValueTask<IReadOnlyDictionary<string, object?>> GetArguments(IRequest request, RoutingMatch match)
    {
        var targetParameters = Operation.Method.GetParameters();

        if (targetParameters.Length > 0)
        {
            var targetArguments = new Dictionary<string, object?>(targetParameters.Length);

            var bodyArguments = FormFormat.GetContent(request);

            for (var i = 0; i < targetParameters.Length; i++)
            {
                var par = targetParameters[i];

                if (par.Name != null)
                {
                    if (Operation.Arguments.TryGetValue(par.Name, out var arg))
                    {
                        targetArguments[arg.Name] = arg.Source switch
                        {
                            OperationArgumentSource.Injected => ArgumentProvider.GetInjectedArgument(request, this, arg, Registry),
                            OperationArgumentSource.Path => ArgumentProvider.GetPathArgument(arg.Name, arg.Type, match, Registry),
                            OperationArgumentSource.Body => await ArgumentProvider.GetBodyArgumentAsync(request, arg.Name, arg.Type, Registry),
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

    private async ValueTask<IResponse?> InterceptAsync(IRequest request, IReadOnlyDictionary<string, object?> arguments)
    {
        if (Operation.Interceptors.Count > 0)
        {
            foreach (var interceptor in Operation.Interceptors)
            {
                if (await interceptor.InterceptAsync(request, Operation, arguments) is IResultWrapper result)
                {
                    return await ResponseProvider.GetResponseAsync(request, Operation, result.Payload, r => result.Apply(r));
                }
            }
        }

        return null;
    }

    private async ValueTask<object?> InvokeAsync(IRequest request, object?[] arguments)
    {
        try
        {
            var instance = await InstanceProvider(request);

            return Operation.Method.Invoke(instance, arguments);
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

        if (!type.IsAsync())
        {
            return result;
        }

        await (result as dynamic);
            
        var resultProperty = result.GetType().GetProperty("Result");

        return resultProperty?.GetValue(result);
    }

    private static async ValueTask<IResponse?> RenderCompilationErrorAsync(IRequest request, CodeGenerationException error)
    {
        var template = new Dictionary<Value, Value>
        {
            ["exception"] = error.InnerException?.ToString() ?? string.Empty,
            ["code"] = error.Code ?? string.Empty
        };
        
        var content = await ErrorRenderer.RenderAsync(template);

        return request.GetPage(content)
                      .Build();
    }

    #endregion

}
