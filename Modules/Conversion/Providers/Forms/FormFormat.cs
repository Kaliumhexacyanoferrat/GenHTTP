using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.Conversion.Providers.Forms
{

    public sealed class FormFormat : ISerializationFormat
    {
        private static readonly Type[] EMPTY_CONSTRUCTOR = Array.Empty<Type>();

        private static readonly object[] EMPTY_ARGS = Array.Empty<object>();

        public async ValueTask<object?> DeserializeAsync(Stream stream, Type type)
        {
            using var reader = new StreamReader(stream);

            var content = await reader.ReadToEndAsync();

            var query = HttpUtility.ParseQueryString(content);

            var constructor = type.GetConstructor(EMPTY_CONSTRUCTOR);

            if (constructor is null)
            {
                throw new ProviderException(ResponseStatus.InternalServerError, $"Instance of type '{type}' cannot be constructed as there is no parameterless constructor");
            }

            var result = constructor.Invoke(EMPTY_ARGS);

            foreach (var key in query.AllKeys)
            {
                if (key is not null)
                {
                    var value = query[key];

                    if (value is not null)
                    {
                        var property = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        if (property is not null)
                        {
                            property.SetValue(result, value.ConvertTo(property.PropertyType));
                        }
                        else
                        {
                            var field = type.GetField(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                            if (field is not null)
                            {
                                field.SetValue(result, value.ConvertTo(field.FieldType));
                            }
                        }
                    }
                }
            }

            return result;
        }

        public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
        {
            var result = request.Respond()
                                .Content(new FormContent(response.GetType(), response))
                                .Type(ContentType.ApplicationWwwFormUrlEncoded);

            return new ValueTask<IResponseBuilder>(result);
        }

        public static Dictionary<string, string>? GetContent(IRequest request)
        {
            if ((request.Content is not null) && (request.ContentType is not null))
            {
                if (request.ContentType == ContentType.ApplicationWwwFormUrlEncoded)
                {
                    var content = GetRequestContent(request);

                    var query = HttpUtility.ParseQueryString(content);

                    var result = new Dictionary<string, string>(query.Count);

                    foreach (var key in query.AllKeys)
                    {
                        var value = query[key];

                        if ((key is not null) && (value is not null))
                        {
                            result.Add(key, value);
                        }
                    }

                    return result;
                }
            }

            return null;
        }

        private static string GetRequestContent(IRequest request)
        {
            var requestContent = request.Content;

            if (requestContent is null)
            {
                throw new InvalidOperationException("Request content has to be set");
            }

            using var reader = new StreamReader(requestContent, Encoding.UTF8, true, 4096, true);

            var content = reader.ReadToEnd();

            request.Content?.Seek(0, SeekOrigin.Begin);

            return content;
        }

    }

}
