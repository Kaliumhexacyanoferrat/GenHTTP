using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules.Websites;

namespace GenHTTP.Modules.Themes.Arcana
{

    public class ArcanaBuilder : IThemeBuilder<ArcanaBuilder>
    {
        private string? _Title, _Copyright, _Footer1Title, _Footer2Title;

        private IBuilder<IMenuProvider>? _Footer1, _Footer2;

        #region Functionality

        public ArcanaBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public ArcanaBuilder Copyright(string copyright)
        {
            _Copyright = copyright;
            return this;
        }

        public ArcanaBuilder Footer1(string title, IBuilder<IMenuProvider> menu)
        {
            _Footer1 = menu;
            _Footer1Title = title;
            return this;
        }

        public ArcanaBuilder Footer2(string title, IBuilder<IMenuProvider> menu)
        {
            _Footer2 = menu;
            _Footer2Title = title;
            return this;
        }

        public ITheme Build()
        {
            return new ArcanaTheme(_Title, _Copyright, _Footer1Title, _Footer1?.Build(), _Footer2Title, _Footer2?.Build());
        }

        #endregion

    }

}
