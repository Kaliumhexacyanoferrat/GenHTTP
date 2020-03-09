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

namespace GenHTTP.Modules.Themes.Lorahost
{

    public class LorahostTheme : ITheme
    {
        private readonly string? _Copyright, _Title, _Subtitle, _Action, _ActionTitle;

        private readonly IRenderer<WebsiteModel> _Renderer;
        
        #region Supporting data structures

        public class ThemeModel
        {

            public string? Copyright { get; }

            public string? Title { get; }

            public string? Subtitle { get; }

            public string? Action { get; }

            public string? ActionTitle { get; }

            public ThemeModel(string? copyright, string? title, string? subtitle, string? action, string? actionTitle)
            {
                Copyright = copyright;
                Title = title;
                Subtitle = subtitle;
                Action = action;
                ActionTitle = actionTitle;
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
                    GetScript("jquery.js"), GetScript("jquery.ajaxchimp.js"), GetScript("mail-script.js"),
                    GetScript("owl.carousel.js"), GetScript("main.js"), GetScript("bootstrap.js")
                };
            }
        }

        public List<Style> Styles
        {
            get
            {
                return new List<Style>
                {
                    GetStyle("bootstrap.css"), GetStyle("fontawesome.css"),
                    GetStyle("owl.carousel.css"), GetStyle("owl.theme.default.css"),
                    GetStyle("themify.css"), GetStyle("style.css")
                };
            }
        }

        public IRouter? Resources { get; }

        #endregion

        #region Initialization

        public LorahostTheme(IResourceProvider? header, string? copyright, string? title, string? subtitle, string? action, string? actionTitle)
        {
            _Copyright = copyright;
            _Title = title;
            _Subtitle = subtitle;
            _Action = action;
            _ActionTitle = actionTitle;

            var resources = Layout.Create()
                                  .Default(Static.Resources("Lorahost.resources"));

            if (header != null)
            {
                resources.Add("header.jpg", Download.From(header).Type(ContentType.ImageJpg));
            }

            Resources = resources.Build();

            _Renderer = new ScribanRenderer<WebsiteModel>(Data.FromResource("Template.html").Build());
        }

        #endregion

        #region Functionality

        public object? GetModel(IRequest request)
        {
            return new ThemeModel(_Copyright, _Title, _Subtitle, _Action, _ActionTitle);
        }

        public IRenderer<WebsiteModel> GetRenderer()
        {
            return _Renderer;
        }

        public IContentProvider? GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            return ModScriban.Page(Data.FromResource("Error.html"), (_) => new ErrorModel(request, responseType, cause))
                             .Build();
        }

        private static Script GetScript(string name)
        {
            return new Script(name, false, Data.FromResource($"scripts.{name}").Build());
        }

        private static Style GetStyle(string name)
        {
            return new Style(name, Data.FromResource($"styles.{name}").Build());
        }

        #endregion

    }

}
