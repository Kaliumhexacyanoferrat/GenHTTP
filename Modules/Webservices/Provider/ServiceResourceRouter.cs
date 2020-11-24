using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Reflection;

namespace GenHTTP.Modules.Webservices.Provider
{

    public sealed class ServiceResourceRouter : IHandler
    {
        private static readonly MethodRouting EMPTY = new("/", "^(/|)$", null, true);

        private static readonly Regex VAR_PATTERN = new(@"\:([a-z]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region Get-/Setters

        private MethodCollection Methods { get; }

        public IHandler Parent { get; }

        public SerializationRegistry Serialization { get; }

        public object Instance { get; }

        #endregion

        #region Initialization

        public ServiceResourceRouter(IHandler parent, object instance, SerializationRegistry formats)
        {
            Parent = parent;

            Instance = instance;
            Serialization = formats;

            Methods = new(this, AnalyzeMethods(instance.GetType()));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

                if (attribute is not null)
                {
                    var path = DeterminePath(attribute);

                    yield return (parent) => new MethodHandler(parent, method, path, () => Instance, attribute, GetResponse, Serialization);
                }
            }
        }

        private MethodRouting DeterminePath(ResourceMethodAttribute metaData)
        {
            var path = metaData.Path;

            if (path is not null)
            {
                var builder = new StringBuilder(path);

                // convert parameters of the format ":var" into appropriate groups
                foreach (Match match in VAR_PATTERN.Matches(path))
                {
                    builder.Replace(match.Value, match.Groups[1].Value.ToParameter());
                }

                var splitted = path.Split("/".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                return new MethodRouting(path, $"^/{builder}$", (splitted.Length > 0) ? splitted[0] : null, false);
            }

            return EMPTY;
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Methods.PrepareAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request) => Methods.GetContent(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

        private async ValueTask<IResponse?> GetResponse(IRequest request, IHandler _, object? result)
        {
            // no result = 204
            if (result is null)
            {
                return request.Respond().Status(ResponseStatus.NoContent).Build();
            }

            var type = result.GetType();

            // response returned by the method
            if (result is IResponseBuilder responseBuilder)
            {
                return responseBuilder.Build();
            }
            else if (result is ValueTask<IResponseBuilder> responseBuilderTask)
            {
                return (await responseBuilderTask).Build();
            }
            else if (result is ValueTask<IResponseBuilder?> optionalResponseBuilderTask)
            {
                return (await optionalResponseBuilderTask)?.Build();
            }

            if (result is IResponse response)
            {
                return response;
            }
            else if (result is ValueTask<IResponse> responseTask)
            {
                return await responseTask;
            }
            else if (result is ValueTask<IResponse?> optionalResponseTask)
            {
                return await optionalResponseTask;
            }

            // stream returned as a download
            if (result is Stream download)
            {
                var downloadResponse = request.Respond()
                                              .Content(download, () => download.CalculateChecksumAsync())
                                              .Type(ContentType.ApplicationForceDownload)
                                              .Build();

                return downloadResponse;
            }

            // basic types should produce a string value
            if (type.IsPrimitive || type == typeof(string) || type.IsEnum || type == typeof(Guid))
            {
                return request.Respond()
                              .Content(result.ToString() ?? string.Empty)
                              .Type(ContentType.TextPlain)
                              .Build();
            }

            // serialize the result
            var serializer = Serialization.GetSerialization(request);

            if (serializer is null)
            {
                throw new ProviderException(ResponseStatus.UnsupportedMediaType, "Requested format is not supported");
            }

            var serializedResult = await serializer.SerializeAsync(request, result);
            
            return serializedResult.Build();
        }

        #endregion

    }

}
