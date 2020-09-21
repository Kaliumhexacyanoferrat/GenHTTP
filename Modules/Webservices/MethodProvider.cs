using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Webservices.Util;

namespace GenHTTP.Modules.Webservices
{

    /// <summary>
    /// Allows to invoke a function on a webservice resource.
    /// </summary>
    /// <remarks>
    /// This provider analyzes the target method to be invoked and supplies
    /// the required arguments. The result of the method is analyzed and
    /// converted into a HTTP response.
    /// </remarks>
    public class MethodProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        /// <summary>
        /// The meta data the method has been annotated with.
        /// </summary>
        public MethodAttribute MetaData { get; }

        /// <summary>
        /// The path of the method, converted into a regular
        /// expression to be evaluated at runtime.
        /// </summary>
        public Regex ParsedPath { get; }

        private MethodInfo Method { get; }

        private object Instance { get; }

        private SerializationRegistry Serialization { get; }

        #endregion

        #region Initialization

        public MethodProvider(IHandler parent, MethodInfo method, object instance, MethodAttribute metaData, SerializationRegistry formats)
        {
            Parent = parent;

            Method = method;
            MetaData = metaData;
            Instance = instance;
            Serialization = formats;

            ParsedPath = PathHelper.Parse(metaData.Path);
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return GetResponse(request, Invoke(request));
        }

        private object? Invoke(IRequest request)
        {
            var targetParameters = Method.GetParameters();

            var targetArguments = new object?[targetParameters.Length];

            var sourceParameters = ParsedPath.Match(request.Target.GetRemaining().ToString());

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

                if (par.ParameterType.IsPrimitive || par.ParameterType == typeof(string) || par.ParameterType.IsEnum)
                {
                    // is there a named parameter?
                    var sourceArgument = sourceParameters.Groups[par.Name];

                    if (sourceArgument.Success)
                    {
                        targetArguments[i] = ChangeType(sourceArgument.Value, par.ParameterType);
                        continue;
                    }

                    // is there a query parameter?
                    if (request.Query.TryGetValue(par.Name, out var value))
                    {
                        targetArguments[i] = ChangeType(value, par.ParameterType);
                        continue;
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

            try
            {
                return Method.Invoke(Instance, targetArguments);
            }
            catch (TargetInvocationException e)
            {
                ExceptionDispatchInfo.Capture(e.InnerException).Throw();
                return null; // nop
            }
        }

        private IResponse GetResponse(IRequest request, object? result)
        {
            // no result = 204
            if (result == null)
            {
                return request.Respond().Status(ResponseStatus.NoContent).Build();
            }

            var type = result.GetType();

            // response returned by the method
            if (result is IResponseBuilder response)
            {
                return response.Build();
            }

            // stream returned as a download
            if (result is Stream download)
            {
                return request.Respond()
                              .Content(download)
                              .Type(ContentType.ApplicationForceDownload)
                              .Build();
            }

            // basic types should produce a string value
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
            {
                return request.Respond().Content(result.ToString())
                                        .Type(ContentType.TextPlain)
                                        .Build();
            }

            // serialize the result
            var serializer = Serialization.GetSerialization(request);

            if (serializer == null)
            {
                throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
            }

            return serializer.Serialize(request, result)
                             .Build();
        }

        private object ChangeType(string value, Type type)
        {
            try
            {
                if (type.IsEnum)
                {
                    return Enum.Parse(type, value);
                }

                return Convert.ChangeType(value, type);
            }
            catch (Exception e)
            {
                throw new ProviderException(ResponseStatus.BadRequest, $"Unable to convert value '{value}' to type '{type}'", e);
            }
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => Enumerable.Empty<ContentElement>();

        #endregion

    }

}
