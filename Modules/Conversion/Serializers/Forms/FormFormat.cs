using System.Reflection;
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

    #endregion

}
