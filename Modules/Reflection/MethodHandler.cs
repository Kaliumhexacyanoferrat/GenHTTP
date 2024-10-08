﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Conversion.Providers.Forms;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Reflection
{

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
        private static readonly object?[] NO_ARGUMENTS = Array.Empty<object?>();

        private static readonly Type? VOID_TASK_RESULT = Type.GetType("System.Threading.Tasks.VoidTaskResult");

        #region Get-/Setters

        public IHandler Parent { get; }

        public MethodRouting Routing { get; }

        public IMethodConfiguration Configuration { get; }

        public MethodInfo Method { get; }

        private Guid ID { get; }

        private Func<object> InstanceProvider { get; }

        private Func<IRequest, IHandler, object?, Action<IResponseBuilder>?, ValueTask<IResponse?>> ResponseProvider { get; }

        private SerializationRegistry Serialization { get; }

        private InjectionRegistry Injection { get; }

        private FormatterRegistry Formatting { get; }

        #endregion

        #region Initialization

        public MethodHandler(IHandler parent, MethodInfo method, MethodRouting routing, Func<object> instanceProvider, IMethodConfiguration metaData,
            Func<IRequest, IHandler, object?, Action<IResponseBuilder>?, ValueTask<IResponse?>> responseProvider, SerializationRegistry serialization,
            InjectionRegistry injection, FormatterRegistry formatting)
        {
            Parent = parent;

            Method = method;
            Configuration = metaData;
            InstanceProvider = instanceProvider;

            Serialization = serialization;
            Injection = injection;
            Formatting = formatting;

            ResponseProvider = responseProvider;

            Routing = routing;

            ID = Guid.NewGuid();
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var arguments = await GetArguments(request);

            var result = Invoke(arguments);

            return await ResponseProvider(request, this, await UnwrapAsync(result), null);
        }

        private async ValueTask<object?[]> GetArguments(IRequest request)
        {
            var targetParameters = Method.GetParameters();

            Match? sourceParameters = null;

            if (!Routing.IsIndex)
            {
                sourceParameters = Routing.ParsedPath.Match(request.Target.GetRemaining().ToString());

                var matchedPath = new PathBuilder(sourceParameters.Value).Build();

                foreach (var _ in matchedPath.Parts) request.Target.Advance();
            }

            if (targetParameters.Length > 0)
            {
                var targetArguments = new object?[targetParameters.Length];

                var bodyArguments = (targetParameters.Length > 0) ? FormFormat.GetContent(request) : null;

                for (int i = 0; i < targetParameters.Length; i++)
                {
                    var par = targetParameters[i];

                    // try to provide via injector
                    var injected = false;

                    foreach (var injector in Injection)
                    {
                        if (injector.Supports(par.ParameterType))
                        {
                            targetArguments[i] = injector.GetValue(this, request, par.ParameterType);

                            injected = true;
                            break;
                        }
                    }

                    if (injected) continue;

                    if ((par.Name is not null) && par.CanFormat(Formatting))
                    {
                        // should the value be read from the body?
                        var fromBody = par.GetCustomAttribute<FromBodyAttribute>();

                        if (fromBody != null)
                        {
                            if (request.Content != null)
                            {
                                using var reader = new StreamReader(request.Content, leaveOpen: true);
                                
                                var body = await reader.ReadToEndAsync();

                                if (!string.IsNullOrWhiteSpace(body))
                                {
                                    targetArguments[i] = body.ConvertTo(par.ParameterType, Formatting);
                                }

                                request.Content.Seek(0, SeekOrigin.Begin);
                            }
                        }
                        else
                        {
                            // is there a named parameter?
                            if (sourceParameters is not null)
                            {
                                var sourceArgument = sourceParameters.Groups[par.Name];

                                if (sourceArgument.Success)
                                {
                                    targetArguments[i] = sourceArgument.Value.ConvertTo(par.ParameterType, Formatting);
                                    continue;
                                }
                            }

                            // is there a query parameter?
                            if (request.Query.TryGetValue(par.Name, out var queryValue))
                            {
                                targetArguments[i] = queryValue.ConvertTo(par.ParameterType, Formatting);
                                continue;
                            }

                            // is there a parameter from the body?
                            if (bodyArguments is not null)
                            {
                                if (bodyArguments.TryGetValue(par.Name, out var bodyValue))
                                {
                                    targetArguments[i] = bodyValue.ConvertTo(par.ParameterType, Formatting);
                                    continue;
                                }
                            }
                        }

                        // assume the default value
                        continue;
                    }
                    else
                    {
                        // deserialize from body
                        var deserializer = Serialization.GetDeserialization(request);

                        if (deserializer is null)
                        {
                            throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
                        }

                        if (request.Content is null)
                        {
                            throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
                        }

                        try
                        {
                            targetArguments[i] = await deserializer.DeserializeAsync(request.Content, par.ParameterType);
                        }
                        catch (Exception e)
                        {
                            throw new ProviderException(ResponseStatus.BadRequest, "Failed to deserialize request body", e);
                        }

                        continue;
                    }
                }

                return targetArguments;
            }

            return NO_ARGUMENTS;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        private object? Invoke(object?[] arguments)
        {
            try
            {
                return Method.Invoke(InstanceProvider(), arguments);
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

                if (type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0] == VOID_TASK_RESULT)
                {
                    return null;
                }

                return task.Result;
            }
            else if (type == typeof(ValueTask) || type == typeof(Task))
            {
                dynamic task = result;

                await task;

                return null;
            }

            return result;
        }

        #endregion

    }

}
