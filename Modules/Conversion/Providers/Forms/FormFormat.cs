using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Basics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace GenHTTP.Modules.Conversion.Providers.Forms
{

    public class FormFormat : ISerializationFormat
    {
        private static readonly Type[] EMPTY_CONSTRUCTOR = new Type[0];

        private static readonly object[] EMPTY_ARGS = new object[0];

        public async Task<object> Deserialize(Stream stream, Type type)
        {
            using var reader = new StreamReader(stream);

            var content = await reader.ReadToEndAsync();

            var query = HttpUtility.ParseQueryString(content);

            var constructor = type.GetConstructor(EMPTY_CONSTRUCTOR);

            if (constructor == null)
            {
                throw new ProviderException(ResponseStatus.InternalServerError, $"Instance of type '{type}' cannot be constructed as there is no parameterless constructor");
            }

            var result = constructor.Invoke(EMPTY_ARGS);

            foreach (var key in query.AllKeys)
            {
                var value = query[key];

                if (!string.IsNullOrWhiteSpace(value))
                {
                    var property = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    if (property != null)
                    {
                        property.SetValue(result, value.ConvertTo(property.PropertyType));
                    }
                    else
                    {
                        var field = type.GetField(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        if (field != null)
                        {
                            field.SetValue(result, value.ConvertTo(field.FieldType));
                        }
                    }
                }
            }

            return result;
        }

        public IResponseBuilder Serialize(IRequest request, object response)
        {
            return request.Respond()
                          .Content(new FormContent(response.GetType(), response))
                          .Type(ContentType.ApplicationWwwFormUrlEncoded);
        }

        public Dictionary<string, string>? GetContent(IRequest request)
        {
            if ((request.Content != null) && (request.ContentType != null))
            {
                if (request.ContentType.Value == ContentType.ApplicationWwwFormUrlEncoded)
                {
                    var content = GetRequestContent(request);

                    var query = HttpUtility.ParseQueryString(content);

                    var result = new Dictionary<string, string>(query.Count);

                    foreach (var key in query.AllKeys)
                    {
                        result.Add(key, query[key]);
                    }

                    return result;
                }
            }

            return null;
        }

        private string GetRequestContent(IRequest request)
        {
            using var reader = new StreamReader(request.Content, Encoding.UTF8, true, 4096, true);

            var content = reader.ReadToEnd();

            request.Content?.Seek(0, SeekOrigin.Begin);

            return content;
        }

    }

}
