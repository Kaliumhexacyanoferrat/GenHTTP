using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.Errors;

namespace GenHTTP.Modules.Core.Websites
{

    public class WebsiteBuilder : IHandlerBuilder<WebsiteBuilder>
    {
        private IHandlerBuilder? _Content;

        private ITheme? _Theme;

        private IBuilder<IMenuProvider>? _Menu;

        private IResourceProvider? _Favicon;

        private readonly StyleRouterBuilder _Styles = new StyleRouterBuilder();

        private readonly ScriptRouterBuilder _Scripts = new ScriptRouterBuilder();

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        private ErrorHandlingProviderBuilder? _ErrorHandling;

        #region Functionality

        public WebsiteBuilder Theme(IBuilder<ITheme> theme) => Theme(theme.Build());

        public WebsiteBuilder Theme(ITheme theme)
        {
            _Theme = theme;
            return this;
        }

        public WebsiteBuilder Content(IHandlerBuilder content)
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
            _Styles.Add(name, provider);
            return this;
        }

        public WebsiteBuilder AddScript(string name, IBuilder<IResourceProvider> provider, bool asynchronous = false) => AddScript(name, provider.Build(), asynchronous);

        public WebsiteBuilder AddScript(string name, IResourceProvider provider, bool asynchronous = false)
        {
            _Scripts.Add(name, provider, asynchronous);
            return this;
        }

        public WebsiteBuilder Add(IConcernBuilder concern)
        {
            if (concern is ErrorHandlingProviderBuilder errorConcern)
            {
                _ErrorHandling = errorConcern;
            }
            else
            {
                _Concerns.Add(concern);
            }

            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var content = _Content ?? throw new BuilderMissingPropertyException("content");

            var theme = _Theme ?? throw new BuilderMissingPropertyException("theme");

            _Styles.Theme(theme);
            _Scripts.Theme(theme);

            var concerns = _Concerns.Concat(new IConcernBuilder[] { _ErrorHandling ?? ErrorHandling.Default() });

            return new WebsiteRouter(parent, content, concerns, _Scripts, _Styles, _Favicon, _Menu?.Build(), theme);
        }

        #endregion

    }

}
