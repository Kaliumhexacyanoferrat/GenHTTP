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

        private IResourceProvider? _Favicon;

        private readonly List<Script> _Scripts = new List<Script>();

        private readonly List<Style> _Styles = new List<Style>();

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

        public WebsiteBuilder Favicon(IBuilder<IResourceProvider> resourceProvider) => Favicon(resourceProvider.Build());

        public WebsiteBuilder Favicon(IResourceProvider resourceProvider)
        {
            _Favicon = resourceProvider;
            return this;
        }

        public WebsiteBuilder AddStyle(string name, IBuilder<IResourceProvider> provider) => AddStyle(name, provider.Build());

        public WebsiteBuilder AddStyle(string name, IResourceProvider provider)
        {
            _Styles.Add(new Style(name, provider));
            return this;
        }

        public WebsiteBuilder AddScript(string name, IBuilder<IResourceProvider> provider, bool asynchronous = false) => AddScript(name, provider.Build(), asynchronous);

        public WebsiteBuilder AddScript(string name, IResourceProvider provider, bool asynchronous = false)
        {
            _Scripts.Add(new Script(name, asynchronous, provider));
            return this;
        }

        public override IRouter Build()
        {
            var content = _Content ?? throw new BuilderMissingPropertyException("content");

            var theme = _Theme ?? new CoreTheme();

            var scripts = new ScriptRouter(theme.Scripts.Union(_Scripts).ToList(), null, null);

            var styles = new StyleRouter(theme.Styles.Union(_Styles).ToList(), null, null);

            var menu = _Menu ?? Core.Menu.From(content);

            var sitemap = Sitemap.From(content).Build();

            return new WebsiteRouter(content, scripts, styles, sitemap, _Favicon, menu.Build(), theme, _ErrorHandler);
        }

        #endregion

    }

}
