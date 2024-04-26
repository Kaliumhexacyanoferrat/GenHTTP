using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;

using GenHTTP.Modules.Minification.Plugins.CSS;
using GenHTTP.Modules.Minification.Plugins.Html;
using GenHTTP.Modules.Minification.Plugins.JS;

namespace GenHTTP.Modules.Minification.Concern
{

    public sealed class MinifyBuilder : IConcernBuilder
    {
        private readonly List<IMinificationPlugin> _Plugins = new();

        #region Functionality

        public MinifyBuilder AddJS() => Add(new JSPlugin());

        public MinifyBuilder AddCss() => Add(new CssPlugin());

        public MinifyBuilder AddHtml() => Add(new HtmlPlugin());

        public MinifyBuilder Add(IMinificationPlugin plugin)
        {
            _Plugins.Add(plugin);
            return this;
        }

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            throw new NotImplementedException();
        }

        #endregion

    }

}
