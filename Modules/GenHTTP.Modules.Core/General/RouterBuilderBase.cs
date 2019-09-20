using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.General
{

    public abstract class RouterBuilderBase<T> : RouterBuilderBase<T, IRouter>, IRouterBuilder { }

    public abstract class RouterBuilderBase<TBuilder, TRouter> : IRouterBuilder<TRouter> where TRouter : IRouter
    {
        protected IRenderer<TemplateModel>? _Template;
        protected IContentProvider? _ErrorHandler;

        #region Functionality

        public TBuilder Template(IBuilder<IRenderer<TemplateModel>> template)
        {
            return Template(template.Build());
        }

        public TBuilder Template(IRenderer<TemplateModel> template)
        {
            _Template = template;
            return (TBuilder)(object)this;
        }

        public TBuilder ErrorHandler(IContentBuilder errorHandler)
        {
            return ErrorHandler(errorHandler.Build());
        }

        public TBuilder ErrorHandler(IContentProvider errorHandler)
        {
            _ErrorHandler = errorHandler;
            return (TBuilder)(object)this;
        }

        public abstract TRouter Build();

        #endregion

    }

}
