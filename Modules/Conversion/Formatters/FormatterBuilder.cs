using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Conversion.Formatters
{

    public sealed class FormatterBuilder : IBuilder<FormatterRegistry>
    {
        private readonly List<IFormatter> _Registry = new();

        #region Functionality

        public FormatterBuilder Add(IFormatter formatter)
        {
            _Registry.Add(formatter);
            return this;
        }

        public FormatterBuilder Add<T>() where T : IFormatter, new() => Add(new T());

        public FormatterRegistry Build()
        {
            return new FormatterRegistry(_Registry);
        }

        #endregion

    }

}
