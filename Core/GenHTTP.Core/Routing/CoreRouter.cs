using System;
using System.Reflection;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

using GenHTTP.Modules.Core.General;
using GenHTTP.Modules.Core.Resource;
using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Core.Routing
{

    internal class CoreRouter : IRouter
    {

        #region Get-/Setters
        
        public IRouter Router { get; }

        public IRouter Parent
        {
            get { return null; }
            set { throw new NotSupportedException("CoreRouter is intended to be the root router."); }
        }

        protected IRenderer<TemplateModel> Template { get; }

        #endregion

        #region Initialization

        internal CoreRouter(IRouter router)
        {
            Router = router;
            Router.Parent = this;

            var templateProvider = new ResourceDataProvider(Assembly.GetExecutingAssembly(), "Template.html");
            Template = new PlaceholderRender<TemplateModel>(templateProvider);
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            Router.HandleContext(current);
        }

        public IRenderer<TemplateModel> GetRenderer()
        {
            return Template;
        }

        public IContentProvider GetErrorHandler(IHttpRequest request, IHttpResponse response)
        {
            var type = response.Header.Type;

            var page = new TemplateModel(request, response, type.ToString(), $"Server returned with response type '{type}'.");

            var content = GetRenderer().Render(page);

            return new StringProvider(content, ContentType.TextHtml);
        }

        #endregion

    }

}
