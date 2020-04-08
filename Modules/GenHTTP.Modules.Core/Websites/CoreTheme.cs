using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

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

        public IHandlerBuilder? Resources => null;

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

        // ToDo: ErrorModel, get renderer here instead (GetErrorRenderer(status))

        public IHandlerBuilder? GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
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

            return Placeholders.Page(Data.FromResource("Error.html"), (_) => new ErrorModel(request, title, body));
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
