using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Conversion;
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

        private string MatchedPathKey { get; }

        private Func<object> InstanceProvider { get; }

        private Func<IRequest, IHandler, object?, ValueTask<IResponse?>> ResponseProvider { get; }

        private SerializationRegistry Serialization { get; }

        private InjectionRegistry Injection { get; }

        #endregion

        #region Initialization

        public MethodHandler(IHandler parent, MethodInfo method, MethodRouting routing, Func<object> instanceProvider, IMethodConfiguration metaData,
            Func<IRequest, IHandler, object?, ValueTask<IResponse?>> responseProvider, SerializationRegistry serialization, InjectionRegistry injection)
        {
            Parent = parent;

            Method = method;
            Configuration = metaData;
            InstanceProvider = instanceProvider;
            Serialization = serialization;
            Injection = injection;

            ResponseProvider = responseProvider;

            Routing = routing;

            ID = Guid.NewGuid();
            MatchedPathKey = $"_{ID}_matchedPath";
        }

        #endregion

        #region Functionality

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            var arguments = await GetArguments(request);

            var result = Invoke(arguments);

            return await ResponseProvider(request, this, await UnwrapAsync(result));
        }

        private async ValueTask<object?[]> GetArguments(IRequest request)
        {
            var targetParameters = Method.GetParameters();

            Match? sourceParameters = null;

            if (Routing.IsIndex)
            {
                request.Properties[MatchedPathKey] = "/";
            }
            else
            {
                sourceParameters = Routing.ParsedPath.Match(request.Target.GetRemaining().ToString());

                var matchedPath = new PathBuilder(sourceParameters.Value).Build();

                foreach (var _ in matchedPath.Parts) request.Target.Advance();

                request.Properties[MatchedPathKey] = matchedPath;
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
                        
                    if ((par.Name is not null) && par.CheckSimple())
                    {
                        // is there a named parameter?
                        if (sourceParameters is not null)
                        {
                            var sourceArgument = sourceParameters.Groups[par.Name];

                            if (sourceArgument.Success)
                            {
                                targetArguments[i] = sourceArgument.Value.ConvertTo(par.ParameterType);
                                continue;
                            }
                        }

                        // is there a query parameter?
                        if (request.Query.TryGetValue(par.Name, out var queryValue))
                        {
                            targetArguments[i] = queryValue.ConvertTo(par.ParameterType);
                            continue;
                        }

                        // is there a parameter from the body?
                        if (bodyArguments is not null)
                        {
                            if (bodyArguments.TryGetValue(par.Name, out var bodyValue))
                            {
                                targetArguments[i] = bodyValue.ConvertTo(par.ParameterType);
                                continue;
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

                        targetArguments[i] = await deserializer.DeserializeAsync(request.Content, par.ParameterType);
                        continue;
                    }
                }

                return targetArguments;
            }

            return NO_ARGUMENTS;
        }

        public WebPath? GetMatchedPath(IRequest request)
        {
            if (request.Properties.TryGet(MatchedPathKey, out WebPath? result))
            {
                return result;
            }

            return null;
        }

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public async IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request)
        {
            if (!Configuration.IgnoreContent)
            {
                if (Configuration.SupportedMethods.Contains(FlexibleRequestMethod.Get(RequestMethod.GET)))
                {
                    foreach (var hint in GetHints(request))
                    {
                        if (TryGetHandler(request, hint, out var handler))
                        {
                            await foreach (var content in handler.GetContentAsync(request))
                            {
                                yield return content;
                            }
                        }
                    }
                }
            }
        }

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

        private List<ContentHint> GetHints(IRequest request)
        {
            if (Configuration.ContentHints is not null)
            {
                if (typeof(IContentHints).IsAssignableFrom(Configuration.ContentHints))
                {
                    var constructor = Configuration.ContentHints.GetConstructor(Array.Empty<Type>());

                    if (constructor is not null)
                    {
                        var obj = constructor.Invoke(Array.Empty<object>());

                        if (obj is IContentHints hints)
                        {
                            return hints.GetHints(request);
                        }
                    }
                }
            }

            return new List<ContentHint>() { new ContentHint() };
        }

        private bool TryGetHandler(IRequest request, ContentHint input, [MaybeNullWhen(returnValue: false)] out IHandler handler)
        {
            if (Method.ReturnType == typeof(IHandlerBuilder) || Method.ReturnType == typeof(IHandler))
            {
                if (TrySimulateArguments(request, input, out var arguments))
                {
                    var result = Invoke(arguments);

                    if (result is IHandlerBuilder builder)
                    {
                        handler = builder.Build(this);
                        return true;
                    }
                    else if (result is IHandler resultHandler)
                    {
                        handler = resultHandler;
                        return true;
                    }
                }
            }

            handler = default;
            return false;
        }

        private bool TrySimulateArguments(IRequest request, ContentHint input, [MaybeNullWhen(returnValue: false)] out object?[] arguments)
        {
            var targetParameters = Method.GetParameters();

            var targetArguments = new object?[targetParameters.Length];

            if (TryResolvePath(input, out var path))
            {
                request.Properties[MatchedPathKey] = path;
            }
            else
            {
                arguments = default;
                return false;
            }

            for (int i = 0; i < targetParameters.Length; i++)
            {
                var par = targetParameters[i];

                // try injectors to provide value
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

                // set from the given input set
                if ((par.Name is not null) && input.TryGetValue(par.Name, out var value))
                {
                    targetArguments[i] = value;
                    continue;
                }
            }

            arguments = targetArguments;
            return true;
        }

        private bool TryResolvePath(ContentHint input, [MaybeNullWhen(returnValue: false)] out WebPath path)
        {
            var resolved = new PathBuilder(Routing.Path.TrailingSlash);

            foreach (var segment in Routing.Path.Parts)
            {
                if (segment.Value.StartsWith(':'))
                {
                    if (input.TryGetValue(segment.Value[1..], out var value))
                    {
                        if (value is not null)
                        {
                            resolved.Append(Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty);
                        }
                        else
                        {
                            path = default;
                            return false;
                        }
                    }
                    else
                    {
                        path = default;
                        return false;
                    }
                }
                else
                {
                    resolved.Append(segment);
                }
            }

            path = resolved.Build();
            return true;
        }

        private static async ValueTask<object?> UnwrapAsync(object? result)
        {
            if (result == null)
            {
                return null;
            }

            var type = result.GetType();
            var genericType = (type.IsGenericType) ? type.GetGenericTypeDefinition() : null;

            if (genericType == typeof(ValueTask<>) || genericType == typeof(Task<>))
            {
                dynamic task = result;

                await task;

                if (type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0] == VOID_TASK_RESULT)
                {
                    return null;
                }

                return task.Result;
            }
            else if (type == typeof(ValueTask) || type == typeof(Task) )
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
