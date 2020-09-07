using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Webservices.Provider
{

    public class ResourceRouter : IHandler
    {
        private static readonly MethodRouting EMPTY = new MethodRouting("/", "^(/|)$", null);

        private static readonly Regex VAR_PATTERN = new Regex(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Get-/Setters

        private MethodCollection Methods { get; }

        public IHandler Parent { get; }

        public SerializationRegistry Serialization { get; }

        public object Instance { get; }

        #endregion

        #region Initialization

        public ResourceRouter(IHandler parent, object instance, SerializationRegistry formats)
        {
            Parent = parent;

            Instance = instance;
            Serialization = formats;

            Methods = new MethodCollection(this, AnalyzeMethods(instance.GetType()));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

                if (attribute != null)
                {
                    var path = DeterminePath(attribute);

                    yield return (parent) => new MethodHandler(parent, method, path, () => Instance, attribute, GetResponse, Serialization);
                }
            }
        }

        private MethodRouting DeterminePath(ResourceMethodAttribute metaData)
        {
            var path = metaData.Path;

            if (path != null)
            {
                var builder = new StringBuilder(path);

                // convert parameters of the format ":var" into appropriate groups
                foreach (Match match in VAR_PATTERN.Matches(path))
                {
                    builder.Replace(match.Value, match.Groups[1].Value.ToParameter());
                }

                var splitted = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                return new MethodRouting(path, $"^/{builder}$", (splitted.Length > 0) ? splitted[0] : null);
            }

            return EMPTY;
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => Methods.GetContent(request);

        public IResponse? Handle(IRequest request) => Methods.Handle(request);

        private IResponse GetResponse(IRequest request, IHandler _, object? result)
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

        #endregion

    }

}
