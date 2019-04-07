using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.General
{

    public abstract class RouterBuilderBase<T> : IRouterBuilder
    {
        protected IRenderer<TemplateModel>? _Template;
        protected IContentProvider? _ErrorHandler;

        #region Functionality

        public T Template(IBuilder<IRenderer<TemplateModel>> template)
        {
            return Template(template.Build());
        }

        public T Template(IRenderer<TemplateModel> template)
        {
            _Template = template;
            return (T)(object)this;
        }

        public T ErrorHandler(IContentBuilder errorHandler)
        {
            return ErrorHandler(errorHandler.Build());
        }

        public T ErrorHandler(IContentProvider errorHandler)
        {
            _ErrorHandler = errorHandler;
            return (T)(object)this;
        }

        public abstract IRouter Build();

        #endregion

    }

}
