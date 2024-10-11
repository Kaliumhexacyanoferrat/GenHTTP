using System;
using System.Collections.Generic;

namespace GenHTTP.Modules.Conversion.Formatters;

public sealed class FormatterRegistry
{

    #region Get-/Setters

    public IReadOnlyList<IFormatter> Formatters { get; private set; }

    #endregion

    #region Initialization

    public FormatterRegistry(List<IFormatter> formatters)
    {
            Formatters = formatters;
        }

    #endregion

    #region Functionality

    public bool CanHandle(Type type)
    {
            for (int i = 0; i < Formatters.Count; i++)
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
            for (int i = 0; i < Formatters.Count; i++)
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
            for (int i = 0; i < Formatters.Count; i++)
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
