using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

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

        #endregion

        #region Initialization

        internal CoreRouter(IRouter content)
        {
            Content = content;
            Content.Parent = this;
        }

        #endregion

        #region Functionality

        public void HandleContext(IEditableRoutingContext current)
        {
            Content.HandleContext(current);
        }

        public IRenderer<TemplateModel> GetRenderer() => throw new InvalidOperationException("Core router has no renderer");

        public IContentProvider GetErrorHandler(IRequest request, ResponseStatus responseType, Exception? cause) => throw new InvalidOperationException("Core router has no error handler");
        
        public IEnumerable<ContentElement> GetContent(IRequest request, string basePath)
        {
            return Content.GetContent(request, basePath);
        }

        public string? Route(string path, int currentDepth)
        {
            return null;
        }

        #endregion

    }

}
