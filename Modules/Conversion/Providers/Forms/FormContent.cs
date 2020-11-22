using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Conversion.Providers.Forms
{

    public class FormContent : IResponseContent
    {

        #region Get-/Setters

        public ulong? Length => null;

        private Type Type { get; }

        private object Data { get; }

        #endregion

        #region Initialization

        public FormContent(Type type, object data)
        {
            Type = type;
            Data = data;
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
                var value = DeriveValue(property.GetValue(Data), property.PropertyType);

                if (value is not null)
                {
                    query[property.Name] = Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }

            foreach (var field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = DeriveValue(field.GetValue(Data), field.FieldType);

                if (value is not null)
                {
                    query[field.Name] = Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }

            var replaced = query?.ToString()?
                                 .Replace("+", "%20")
                                 .Replace("%2b", "+");

            await writer.WriteAsync(replaced);
        }

        private static object? DeriveValue(object? value, Type sourceType)
        {
            if (sourceType == typeof(bool) && value is not null)
            {
                return ((bool)value) ? 1 : 0;
            }

            return value;
        }

        #endregion

    }

}
