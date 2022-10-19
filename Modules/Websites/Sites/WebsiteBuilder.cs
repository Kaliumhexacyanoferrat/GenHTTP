using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Websites;

using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Websites.Resources;

namespace GenHTTP.Modules.Websites.Sites
{

    public sealed class WebsiteBuilder : IHandlerBuilder<WebsiteBuilder>
    {
        private IHandlerBuilder? _Content;

        private ITheme? _Theme;

        private IBuilder<IMenuProvider>? _Menu;

        private IResource? _Favicon;

        private readonly StyleRouterBuilder _Styles = new();

        private readonly ScriptRouterBuilder _Scripts = new();

        private readonly List<IConcernBuilder> _Concerns = new();

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

        public WebsiteBuilder Favicon(IBuilder<IResource> resourceProvider) => Favicon(resourceProvider.Build());

        public WebsiteBuilder Favicon(IResource resourceProvider)
        {
            _Favicon = resourceProvider;
            return this;
        }

        public WebsiteBuilder AddStyle(string name, IBuilder<IResource> provider) => AddStyle(name, provider.Build());

        public WebsiteBuilder AddStyle(string name, IResource provider)
        {
            _Styles.Add(name, provider);
            return this;
        }

        public WebsiteBuilder AddScript(string name, IBuilder<IResource> provider, bool asynchronous = false) => AddScript(name, provider.Build(), asynchronous);

        public WebsiteBuilder AddScript(string name, IResource provider, bool asynchronous = false)
        {
            _Scripts.Add(name, provider, asynchronous);
            return this;
        }

        public WebsiteBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);

            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var content = _Content ?? throw new BuilderMissingPropertyException("content");

            var theme = _Theme ?? throw new BuilderMissingPropertyException("theme");

            _Styles.Theme(theme);
            _Scripts.Theme(theme);

            return new WebsiteRouter(parent, content, _Concerns, _Scripts, _Styles, _Favicon, _Menu?.Build(), theme);
        }

        #endregion

    }

}
