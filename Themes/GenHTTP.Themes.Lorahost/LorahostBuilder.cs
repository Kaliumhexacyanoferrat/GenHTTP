using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Websites;

namespace GenHTTP.Modules.Themes.Lorahost
{

    public class LorahostBuilder : IThemeBuilder<LorahostTheme>
    {
        private string? _Copyright, _Title, _Subtitle, _Action, _ActionTitle;

        private IResourceProvider? _Header;

        #region Functionality

        public LorahostBuilder Copyright(string copyright)
        {
            _Copyright = copyright;
            return this;
        }

        public LorahostBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public LorahostBuilder Subtitle(string subtitle)
        {
            _Subtitle = subtitle;
            return this;
        }

        public LorahostBuilder Action(string path, string text)
        {
            _Action = path;
            _ActionTitle = text;
            return this;
        }

        public LorahostBuilder Header(IBuilder<IResourceProvider> headerProvider) => Header(headerProvider.Build());

        public LorahostBuilder Header(IResourceProvider headerProvider)
        {
            _Header = headerProvider;
            return this;
        }

        public ITheme Build()
        {
            return new LorahostTheme(_Header, _Copyright, _Title, _Subtitle, _Action, _ActionTitle);
        }

        #endregion

    }

}
