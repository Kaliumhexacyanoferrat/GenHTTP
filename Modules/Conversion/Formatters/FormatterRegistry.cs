namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class FormatterRegistry
{

    #region Initialization

    public FormatterRegistry(List<IFormatter> formatters)
    {
        Formatters = formatters;
    }

    #endregion

    #region Get-/Setters

    public IReadOnlyList<IFormatter> Formatters { get; }

    #endregion

    #region Functionality

    public bool CanHandle(Type type)
    {
        for (var i = 0; i < Formatters.Count; i++)
        {
            if (Formatters[i].CanHandle(type))
            {
                return true;
            }
        }

        return false;
    }

    public object? Read(string value, Type type)
    {
        for (var i = 0; i < Formatters.Count; i++)
        {
            if (Formatters[i].CanHandle(type))
            {
                return Formatters[i].Read(value, type);
            }
        }

        return null;
    }


    public string? Write(object value, Type type)
    {
        for (var i = 0; i < Formatters.Count; i++)
        {
            if (Formatters[i].CanHandle(type))
            {
                return Formatters[i].Write(value, type);
            }
        }

        return null;
    }

    #endregion

}
