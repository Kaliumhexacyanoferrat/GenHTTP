using System;
using System.Reflection;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;
using GenHTTP.Modules.Core.Resource;
using GenHTTP.Modules.Core.Templating;

using GenHTTP.Core.Utilities;

namespace GenHTTP.Core.Routing
{

    internal class CoreRouter : IRouter
    {

        #region Get-/Setters

        public IRouter Content { get; }

        public IRouter Parent
        {
            get { throw new NotSupportedException("Core router has no parent"); }
            set { throw new NotSupportedException("Setting core router's parent is not allowed"); }
        }

        private IRenderer<TemplateModel> Template { get; }

        #endregion

        #region Initialization

        internal CoreRouter(IRouter content)
        {
            Content = content;
            Content.Parent = this;

            var templateProvider = new ResourceDataProvider(Assembly.GetExecutingAssembly(), "Template.html");
            Template = new PlaceholderRender<TemplateModel>(templateProvider);
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            Content.HandleContext(current);
        }

        public IRenderer<TemplateModel> GetRenderer()
        {
            return Template;
        }

        public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause)
        {
            var title = responseType.ToString().SplitCamelCase();

            string body;

            if (request.Server.Development && (cause != null))
            {
                body = cause.ToString().Replace(Environment.NewLine, "<br />");
            }
            else
            {
                body = $"Server returned with response type '{title}'.";
            }

            var page = new TemplateModel(request, title, body);

            var renderer = request.Routing?.Router.GetRenderer() ?? GetRenderer();

            var content = renderer.Render(page);

            return new StringProvider(content, ContentType.TextHtml, null);
        }

        public string? Route(string path, int currentDepth)
        {
            return null;
        }

        #endregion

    }

}
