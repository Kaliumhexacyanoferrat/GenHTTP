using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Conversion.Providers.Forms;

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
    public class MethodHandler : IHandler
    {
        private static FormFormat _FormFormat = new FormFormat();

        #region Get-/Setters

        public IHandler Parent { get; }

        public MethodRouting Routing { get; }

        public MethodAttribute MetaData { get; }

        public MethodInfo Method { get; }

        private Guid ID { get; }

        private Func<object> InstanceProvider { get; }

        private Func<IRequest, IHandler, object?, IResponse?> ResponseProvider { get; }

        private SerializationRegistry Serialization { get; }

        #endregion

        #region Initialization

        public MethodHandler(IHandler parent, MethodInfo method, MethodRouting routing, Func<object> instanceProvider, MethodAttribute metaData,
            Func<IRequest, IHandler, object?, IResponse?> responseProvider, SerializationRegistry serialization)
        {
            Parent = parent;

            Method = method;
            MetaData = metaData;
            InstanceProvider = instanceProvider;
            Serialization = serialization;

            ResponseProvider = responseProvider;

            Routing = routing;

            ID = Guid.NewGuid();
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            var arguments = GetArguments(request);

            return ResponseProvider(request, this, Invoke(arguments));
        }

        private object?[] GetArguments(IRequest request)
        {
            var targetParameters = Method.GetParameters();

            var targetArguments = new object?[targetParameters.Length];

            var sourceParameters = Routing.ParsedPath.Match(request.Target.GetRemaining().ToString());

            var matchedPath = new PathBuilder(sourceParameters.Value).Build();

            foreach (var _ in matchedPath.Parts) request.Target.Advance();

            request.Properties[$"_{ID}_matchedPath"] = matchedPath;

            var bodyArguments = (targetParameters.Length > 0) ? _FormFormat.GetContent(request) : null;

            for (int i = 0; i < targetParameters.Length; i++)
            {
                var par = targetParameters[i];

                // request
                if (par.ParameterType == typeof(IRequest))
                {
                    targetArguments[i] = request;
                    continue;
                }

                // handler
                if (par.ParameterType == typeof(IHandler))
                {
                    targetArguments[i] = this;
                    continue;
                }

                // input stream
                if (par.ParameterType == typeof(Stream))
                {
                    if (request.Content == null)
                    {
                        throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
                    }

                    targetArguments[i] = request.Content;
                    continue;
                }

                if (par.CheckSimple())
                {
                    // is there a named parameter?
                    var sourceArgument = sourceParameters.Groups[par.Name];

                    if (sourceArgument.Success)
                    {
                        targetArguments[i] = sourceArgument.Value.ConvertTo(par.ParameterType);
                        continue;
                    }

                    // is there a query parameter?
                    if (request.Query.TryGetValue(par.Name, out var queryValue))
                    {
                        targetArguments[i] = queryValue.ConvertTo(par.ParameterType);
                        continue;
                    }

                    // is there a parameter from the body?
                    if (bodyArguments != null)
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

                    if (deserializer == null)
                    {
                        throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
                    }

                    if (request.Content == null)
                    {
                        throw new ProviderException(ResponseStatus.BadRequest, "Request body expected");
                    }

                    targetArguments[i] = Task.Run(async () => await deserializer.Deserialize(request.Content, par.ParameterType)).Result;
                    continue;
                }
            }

            return targetArguments;
        }

        public WebPath? GetMatchedPath(IRequest request)
        {
            if (request.Properties.TryGet($"_{ID}_matchedPath", out WebPath result))
            {
                return result;
            }

            return null;
        }

        private object? Invoke(object?[] arguments)
        {
            try
            {
                return Method.Invoke(InstanceProvider(), arguments);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                return null; // nop
            }
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            if (!MetaData.IgnoreContent)
            {
                if (MetaData.SupportedMethods.Contains(new FlexibleRequestMethod(RequestMethod.GET)))
                {
                    foreach (var hint in GetHints(request))
                    {
                        if (TryGetHandler(request, hint, out var handler))
                        {
                            foreach (var content in handler.GetContent(request))
                            {
                                yield return content;
                            }
                        }
                    }
                }
            }
        }

        private List<ContentHint> GetHints(IRequest request)
        {
            if (MetaData.ContentHints != null)
            {
                if (typeof(IContentHints).IsAssignableFrom(MetaData.ContentHints))
                {
                    var constructor = MetaData.ContentHints.GetConstructor(new Type[0]);

                    if (constructor != null)
                    {
                        var obj = constructor.Invoke(new object[0]);

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
                request.Properties[$"_{ID}_matchedPath"] = path;
            }
            else
            {
                arguments = default;
                return false;
            }

            for (int i = 0; i < targetParameters.Length; i++)
            {
                var par = targetParameters[i];

                // request
                if (par.ParameterType == typeof(IRequest))
                {
                    targetArguments[i] = request;
                    continue;
                }

                // handler
                if (par.ParameterType == typeof(IHandler))
                {
                    targetArguments[i] = this;
                    continue;
                }

                // set from the given input set
                if (input.TryGetValue(par.Name, out var value))
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
                if (segment.StartsWith(':'))
                {
                    if (input.TryGetValue(segment.Substring(1), out var value))
                    {
                        if (value != null)
                        {
                            resolved.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
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

        #endregion

    }

}
