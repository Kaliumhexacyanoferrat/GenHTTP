using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core.Resource;
using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core.Websites
{

    public class CoreTheme : ITheme
    {

        #region Supporting data structures

        private class ErrorModel : PageModel
        {

            public string Message { get; }

            public ErrorModel(IRequest request, string title, string message) : base(request)
            {
                Title = title;
                Message = message;
            }

        }

        #endregion

        #region Get-/Setters

        public List<Script> Scripts => new List<Script>();

        public List<Style> Styles => new List<Style>();

        public IRouter? Resources => null;

        private IRenderer<WebsiteModel> Template { get; }

        #endregion

        #region Initialization

        public CoreTheme()
        {
            var templateProvider = new ResourceDataProvider(Assembly.GetExecutingAssembly(), "Template.html");
            Template = new PlaceholderRender<WebsiteModel>(templateProvider);
        }

        #endregion

        #region Functionality

        public IContentProvider? GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            var title = SplitCamelCase(responseType.ToString());

            string body;

            if (request.Server.Development && (cause != null))
            {
                body = cause.ToString().Replace(Environment.NewLine, "<br />");
            }
            else
            {
                body = $"Server returned with response type '{title}'.";
            }

            return Placeholders.Page(Data.FromResource("Error.html"), (_) => new ErrorModel(request, title, body))
                               .Build();
        }

        public IRenderer<WebsiteModel> GetRenderer() => Template;

        public object? GetModel(IRequest request) => null;

        private static string SplitCamelCase(string str)
        {
            return Regex.Replace(
                Regex.Replace(
                    str,
                    @"(\P{Ll})(\P{Ll}\p{Ll})",
                    "$1 $2"
                ),
                @"(\p{Ll})(\P{Ll})",
                "$1 $2"
            );
        }
        
        #endregion

    }

}
