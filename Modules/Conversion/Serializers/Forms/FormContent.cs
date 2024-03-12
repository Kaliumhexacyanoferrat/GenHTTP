using System;
using System.Collections.Specialized;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Formatters;

namespace GenHTTP.Modules.Conversion.Providers.Forms
{

    public sealed class FormContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => null;

        private Type Type { get; }

        private object Data { get; }

        private FormatterRegistry Formatters { get; }

        #endregion

        #region Initialization

        public FormContent(Type type, object data, FormatterRegistry formatters)
        {
            Type = type;
            Data = data;

            Formatters = formatters;
        }

        #endregion

        #region Functionality

        public ValueTask<ulong?> CalculateChecksumAsync()
        {
            return new ValueTask<ulong?>((ulong)Data.GetHashCode());
        }

        public async ValueTask WriteAsync(Stream target, uint bufferSize)
        {
            using var writer = new StreamWriter(target, Encoding.UTF8, (int)bufferSize, true);

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var property in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                SetValue(query, property.Name, property.GetValue(Data), property.PropertyType);
            }

            foreach (var field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                SetValue(query, field.Name, field.GetValue(Data), field.FieldType);
            }

            var replaced = query?.ToString()?
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

        #endregion

    }

}
