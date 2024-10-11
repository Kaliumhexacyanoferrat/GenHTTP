using System;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class StringFormatter : IFormatter
{

    public bool CanHandle(Type type) => type == typeof(string);

    public object? Read(string value, Type type) => value;

    public string? Write(object value, Type type) => (string)value;

}
