using System;

namespace GenHTTP.Modules.Conversion.Formatters
{

    public sealed class BoolFormatter : IFormatter
    {

        public bool CanHandle(Type type) => type == typeof(bool);

        public object? Read(string value, Type type)
        {
            if (value == "1" || value == "on")
            {
                return true;
            }
            else if (value == "0" || value == "off") 
            {
                return false; 
            }

            return null;
        }

        public string? Write(object value, Type type) => ((bool)value) ? "1" : "0";

    }

}
