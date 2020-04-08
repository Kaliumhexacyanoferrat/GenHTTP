using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenHTTP.Modules.Core.Websites
{

    public class StyleRouterBuilder : IHandlerBuilder
    {
        private readonly List<Style> _Styles = new List<Style>();

        private ITheme? _Theme;

        #region Functionality

        public StyleRouterBuilder Add(string name, IResourceProvider provider)
        {
            _Styles.Add(new Style(name, provider));
            return this;
        }

        public StyleRouterBuilder Theme(ITheme theme)
        {
            _Theme = theme;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var styles = (_Theme != null) ? _Theme.Styles.Union(_Styles) : _Styles;

            return new StyleRouter(parent, styles.ToList());
        }

        #endregion

    }

}
