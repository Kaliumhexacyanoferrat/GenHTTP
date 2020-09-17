using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;

namespace GenHTTP.Modules.Websites.Resources
{

    public class StyleRouterBuilder : IHandlerBuilder<StyleRouterBuilder>
    {
        private readonly List<Style> _Styles = new List<Style>();

        private ITheme? _Theme;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public StyleRouterBuilder Add(string name, IResourceProvider provider)
        {
            _Styles.Add(new Style(name, provider));
            return this;
        }

        public StyleRouterBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public StyleRouterBuilder Theme(ITheme theme)
        {
            _Theme = theme;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var styles = _Theme != null ? _Theme.Styles.Union(_Styles) : _Styles;

            return new StyleRouter(parent, styles.ToList());
        }

        #endregion

    }

}
