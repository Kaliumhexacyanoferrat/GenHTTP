using System;
using System.Collections.Generic;
using System.Linq;

namespace GenHTTP.Modules.Conversion.Formatters
{

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

        public bool CanHandle(Type type) => Formatters.Any(f => f.CanHandle(type));

        public object? Read(string value, Type type) => Formatters.First(f => f.CanHandle(type)).Read(value, type); 
        
        public string? Write(object value, Type type) => Formatters.First(f => f.CanHandle(type)).Write(value, type);

        #endregion

    }

}
