using System;
using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core;
using GenHTTP.Modules.Core.Websites;
using GenHTTP.Modules.Scriban;

namespace GenHTTP.Modules.Themes.Arcana
{

    public class ArcanaTheme : ITheme
    {
        private readonly string? _Title, _Copyright, _Footer1Title, _Footer2Title;

        private readonly IMenuProvider? _FooterMenu1, _FooterMenu2;

        private readonly IRenderer<WebsiteModel> _Renderer;

        #region Supporting data structures

        public class ThemeModel
        {

            public string? Title { get; }

            public string? Copyright { get; }

            public string? Title1 { get; }

            public List<ContentElement>? Footer1 { get; }

            public string? Title2 { get; }

            public List<ContentElement>? Footer2 { get; }

            public ThemeModel(string? title, string? copyright, string? footer1Title, List<ContentElement>? footer1, string? footer2Title, List<ContentElement>? footer2)
            {
                Title = title;
                Copyright = copyright;

                Footer1 = footer1;
                Title1 = footer1Title;

                Footer2 = footer2;
                Title2 = footer2Title;
            }

        }

        #endregion

        #region Get-/Setters

        public List<Script> Scripts
        {
            get
            {
                return new List<Script>
                {
                    GetScript("jquery.js"), GetScript("jquery.dropotron.js"),
                    GetScript("browser.js"), GetScript("breakpoints.js"),
                    GetScript("util.js"), GetScript("main.js")
                };
            }
        }

        public List<Style> Styles
        {
            get { return new List<Style> { GetStyle("fontawesome.css"), GetStyle("main.css") }; }
        }

        public IRouter Resources { get; }

        #endregion

        #region Initialization

        public ArcanaTheme(string? title, string? copyright, string? footer1Title, IMenuProvider? footerMenu1, string? footer2Title, IMenuProvider? footerMenu2)
        {
            _Title = title;
            _Copyright = copyright;

            _FooterMenu1 = footerMenu1;
            _Footer1Title = footer1Title;

            _FooterMenu2 = footerMenu2;
            _Footer2Title = footer2Title;

            Resources = Resources = Static.Resources("Arcana.resources").Build();

            _Renderer = new ScribanRenderer<WebsiteModel>(Data.FromResource("Template.html").Build());
        }

        #endregion

        #region Functionality

        public IRenderer<WebsiteModel> GetRenderer()
        {
            return _Renderer;
        }

        private static Script GetScript(string name)
        {
            return new Script(name, false, Data.FromResource($"scripts.{name}").Build());
        }

        private static Style GetStyle(string name)
        {
            return new Style(name, Data.FromResource($"styles.{name}").Build());
        }

        public IContentProvider? GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            return ModScriban.Page(Data.FromResource("Error.html"), (_) => new ErrorModel(request, responseType, cause))
                             .Build();
        }

        public object? GetModel(IRequest request)
        {
            var footer1 = _FooterMenu1?.GetMenu(request);
            var footer2 = _FooterMenu2?.GetMenu(request);

            return new ThemeModel(_Title, _Copyright, _Footer1Title, footer1, _Footer2Title, footer2);
        }

        #endregion

    }

}
