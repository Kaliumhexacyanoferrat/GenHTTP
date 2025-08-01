﻿using System.Reflection;
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
    private static readonly Dictionary<string, object?> NoArguments = [];

    #region Get-/Setters

    public Operation Operation { get; }

    public IMethodConfiguration Configuration { get; }

    private Func<IRequest, ValueTask<object>> InstanceProvider { get; }

    public MethodRegistry Registry { get; }

    private ResponseProvider ResponseProvider { get; }

    #endregion

    #region Initialization

    /// <summary>
    /// Creates a new handler to serve a single API operation.
    /// </summary>
    /// <param name="operation">The operation to be executed and provided (use <see cref="OperationBuilder"/> to create an operation)</param>
    /// <param name="instanceProvider">A factory that will provide an instance to actually execute the operation on</param>
    /// <param name="metaData">Additional, use-specified information about the operation</param>
    /// <param name="registry">The customized registry to be used to read and write data</param>
    public MethodHandler(Operation operation, Func<IRequest, ValueTask<object>> instanceProvider, IMethodConfiguration metaData, MethodRegistry registry)
    {
        Configuration = metaData;
        InstanceProvider = instanceProvider;

        Operation = operation;
        Registry = registry;

        ResponseProvider = new(registry);
    }

    #endregion

    #region Functionality

    public async ValueTask<IResponse?> HandleAsync(IRequest request)
    {
        var arguments = await GetArguments(request);

        var interception = await InterceptAsync(request, arguments);

        if (interception is not null)
        {
            return interception;
        }

        var result = await InvokeAsync(request, arguments.Values.ToArray());

        return await ResponseProvider.GetResponseAsync(request, Operation, await UnwrapAsync(result), null);
    }

    private async ValueTask<IReadOnlyDictionary<string, object?>> GetArguments(IRequest request)
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
