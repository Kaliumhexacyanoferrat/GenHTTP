using System.Reflection;
using System.Text;
using System.Web;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Conversion.Serializers.Forms;

public sealed class FormFormat : ISerializationFormat
{
    private static readonly Type[] EmptyConstructor = [];

    private static readonly object[] EmptyArgs = [];

    #region Get-/Setters

    private FormatterRegistry Formatters { get; }

    #endregion

    #region Initialization

    public FormFormat(FormatterRegistry formatters)
    {
        Formatters = formatters;
    }

    public FormFormat() : this(Formatting.Default().Build()) { }

    #endregion

    #region Functionality

    public async ValueTask<object?> DeserializeAsync(Stream stream, Type type)
    {
        using var reader = new StreamReader(stream);

        var content = await reader.ReadToEndAsync();

        var query = HttpUtility.ParseQueryString(content);

        var constructor = type.GetConstructor(EmptyConstructor);

        if (constructor is null)
        {
            throw new ProviderException(ResponseStatus.InternalServerError, $"Instance of type '{type}' cannot be constructed as there is no parameterless constructor");
        }

        var result = constructor.Invoke(EmptyArgs);

        foreach (var key in query.AllKeys)
        {
            if (key is not null)
            {
                var value = query[key];

                if (value is not null)
                {
                    var property = type.GetProperty(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                    // very inefficient, but the form format is probably not used that much
                    // so we do not optimize that for now
                    ByteString byteValue = new(value);
                    
                    if (property is not null)
                    {
                        property.SetValue(result, byteValue.ConvertTo(property.PropertyType, Formatters));
                    }
                    else
                    {
                        var field = type.GetField(key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

                        field?.SetValue(result, byteValue.ConvertTo(field.FieldType, Formatters));
                    }
                }
            }
        }

        return result;
    }

    public ValueTask<IResponseBuilder> SerializeAsync(IRequest request, object response)
    {
        var result = request.Respond()
                            .Content(new FormContent(response.GetType(), response, Formatters));

        return new ValueTask<IResponseBuilder>(result);
    }

    public ValueTask<ReadOnlyMemory<byte>> SerializeAsync(object data)
    {
        return ByteStreamSerialization.SerializeAsync(b =>
        {
            var content = new FormContent(data.GetType(), data, Formatters);

            return content.WriteAsync(b);
        });
    }

    public static async ValueTask<Dictionary<string, string>?> GetContentAsync(IRequest request) // todo: refactor into an own type
    {
        var contentType = request.Header.Headers.GetEntry(KnownHeaders.ContentType);

        if (contentType is not null)
        {
            if (new ContentType(contentType.Value.Bytes) == ContentType.ApplicationWwwFormUrlEncoded) // todo: ugly API
            {
                var content = await GetRequestContentAsync(request); // todo: make this memory based?

                var query = HttpUtility.ParseQueryString(content);

                var result = new Dictionary<string, string>(query.Count);

                foreach (var key in query.AllKeys)
                {
                    var value = query[key];

                    if (key is not null && value is not null)
                    {
                        result.Add(key, value);
                    }
                }

                return result;
            }
        }

        return null;
    }

    private static async ValueTask<string> GetRequestContentAsync(IRequest request)
    {
        var requestContent = request.GetBody();

        if (requestContent is null)
        {
            throw new InvalidOperationException("Request content has to be set");
        }

        var buffer = await requestContent.AsMemoryAsync();

        return Encoding.UTF8.GetString(buffer.Span);
    }

    #endregion

}
