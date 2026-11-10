using System.Collections.Specialized;
using System.Reflection;
using System.Web;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.IO.Pooling;

namespace GenHTTP.Modules.Conversion.Serializers.Forms;

public sealed class FormContent : PooledContent<FormContent>, IResponseContent
{
    private static readonly object NoData = new();

    private static readonly FormatterRegistry NoFormatters = Formatting.Empty().Build();

    #region Initialization

    public FormContent(Type type, object data, FormatterRegistry formatters)
    {
        DataType = type;
        Data = data;

        Formatters = formatters;
    }

    internal static FormContent Rent(Type type, object data, FormatterRegistry formatters)
    {
        var content = FromPool(static () => new FormContent(typeof(object), NoData, NoFormatters));

        content.DataType = type;
        content.Data = data;
        content.Formatters = formatters;

        return content;
    }

    #endregion

    #region Get-/Setters

    public ulong? Length => null;

    public ContentType? Type => ContentType.ApplicationWwwFormUrlEncoded;

    public ReadOnlyMemory<byte>? Encoding => null;

    private Type DataType { get; set; }

    private object Data { get; set; }

    private FormatterRegistry Formatters { get; set; }

    #endregion

    #region Functionality

    public ValueTask<ulong?> CalculateChecksumAsync() => new((ulong)Data.GetHashCode());

    public ValueTask WriteAsync(IResponseSink sink) => WriteAsync(sink.Stream);

    public async ValueTask WriteAsync(Stream target)
    {
        await using var writer = new StreamWriter(target, System.Text.Encoding.UTF8, 4096, true);

        var query = HttpUtility.ParseQueryString(string.Empty);

        foreach (var property in DataType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            SetValue(query, property.Name, property.GetValue(Data), property.PropertyType);
        }

        foreach (var field in DataType.GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            SetValue(query, field.Name, field.GetValue(Data), field.FieldType);
        }

        var replaced = query.ToString()?
                            .Replace("+", "%20")
                            .Replace("%2b", "+");

        await writer.WriteAsync(replaced);
    }



    private void SetValue(NameValueCollection query, string field, object? value, Type type)
    {
        if (value != null)
        {
            var formatted = Formatters.Write(value, type);

            if (formatted is not null)
            {
                query[field] = formatted;
            }
        }
    }

    protected override void Reset()
    {
        DataType = typeof(object);
        Data = NoData;
        Formatters = NoFormatters;
    }

    #endregion

}
