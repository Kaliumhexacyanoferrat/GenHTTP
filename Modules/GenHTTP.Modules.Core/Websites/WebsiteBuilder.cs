using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Core.Websites
{

    public class WebsiteBuilder : RouterBuilderBase<WebsiteBuilder>
    {
        private IRouter? _Content;

        private ITheme? _Theme;

        private IBuilder<IMenuProvider>? _Menu;

        private List<Script> _Scripts = new List<Script>();

        private List<Style> _Styles = new List<Style>();

        #region Functionality

        public WebsiteBuilder Theme(IBuilder<ITheme> theme) => Theme(theme.Build());

        public WebsiteBuilder Theme(ITheme theme)
        {
            _Theme = theme;
            return this;
        }

        public WebsiteBuilder Content(IRouterBuilder content)
        {
            return Content(content.Build());
        }

        public WebsiteBuilder Content(IRouter content)
        {
            _Content = content;
            return this;
        }

        public WebsiteBuilder Menu(IBuilder<IMenuProvider> menu)
        {
            _Menu = menu;
            return this;
        }

        public WebsiteBuilder AddStyle(string name, IBuilder<IResourceProvider> provider) => AddStyle(name, provider.Build());

        public WebsiteBuilder AddStyle(string name, IResourceProvider provider)
        {
            _Styles.Add(new Style(name, provider));
            return this;
        }

        public WebsiteBuilder AddScript(string name, IBuilder<IResourceProvider> provider, bool async = false) => AddScript(name, provider.Build(), async);

        public WebsiteBuilder AddScript(string name, IResourceProvider provider, bool async = false)
        {
            _Scripts.Add(new Script(name, async, provider));
            return this;
        }

        public override IRouter Build()
        {
            var content = _Content ?? throw new BuilderMissingPropertyException("content");

            var theme = _Theme ?? new CoreTheme();

            var scripts = new ScriptRouter(theme.Scripts.Union(_Scripts).ToList(), null, null);

            var styles = new StyleRouter(theme.Styles.Union(_Styles).ToList(), null, null);

            var menu = _Menu ?? Core.Menu.From(content);

            return new WebsiteRouter(content, scripts, styles, menu.Build(), theme, _ErrorHandler);
        }

        #endregion

    }

}
