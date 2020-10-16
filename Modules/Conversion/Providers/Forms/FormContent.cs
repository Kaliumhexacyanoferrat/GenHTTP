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

        public ulong? Checksum => (ulong)Data.GetHashCode();

        #endregion

        #region Initialization

        public FormContent(Type type, object data)
        {
            Type = type;
            Data = data;
        }

        #endregion

        #region Functionality

        public async Task Write(Stream target, uint bufferSize)
        {
            using var writer = new StreamWriter(target, Encoding.UTF8, (int)bufferSize, true);

            var query = HttpUtility.ParseQueryString(string.Empty);

            foreach (var property in Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = DeriveValue(property.GetValue(Data), property.PropertyType);

                if (value != null)
                {
                    query[property.Name] = Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }

            foreach (var field in Type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var value = DeriveValue(field.GetValue(Data), field.FieldType);

                if (value != null)
                {
                    query[field.Name] = Convert.ToString(value, CultureInfo.InvariantCulture);
                }
            }

            var replaced = query.ToString()
                                .Replace("+", "%20")
                                .Replace("%2b", "+");

            await writer.WriteAsync(replaced);
        }

        private object? DeriveValue(object? value, Type sourceType)
        {
            if (sourceType == typeof(bool) && value != null)
            {
                return ((bool)value) ? 1 : 0;
            }

            return value;
        }

        #endregion

    }

}
