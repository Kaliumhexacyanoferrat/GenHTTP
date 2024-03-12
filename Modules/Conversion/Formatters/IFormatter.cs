using System;

namespace GenHTTP.Modules.Conversion.Formatters
{

    public interface IFormatter
    {

        bool CanHandle(Type type);

        object? Read(string value, Type type);

        string? Write(object value, Type type);

    }

}
