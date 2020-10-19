using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;
using GenHTTP.Modules.IO;
using GenHTTP.Modules.Placeholders;

namespace GenHTTP.Engine.Infrastructure
{

    internal class CoreRouter : IHandler, IErrorHandler, IPageRenderer
    {

        #region Get-/Setters

        public IHandler Parent
        {
            get { throw new NotSupportedException("Core router has no parent"); }
            set { throw new NotSupportedException("Setting core router's parent is not allowed"); }
        }

        public IHandler Content { get; }

        private IRenderer<TemplateModel> Template { get; }

        private IRenderer<ErrorModel> ErrorRenderer { get; }

        #endregion

        #region Initialization

        internal CoreRouter(IHandlerBuilder content, IEnumerable<IConcernBuilder> concerns, bool development)
        {
            Content = Concerns.Chain(this, concerns, (p) => content.Build(p));

            Template = Placeholders.Template<TemplateModel>(Resource.FromAssembly("Template.html"))
                                   .Build();

            ErrorRenderer = Placeholders.Template<ErrorModel>(Resource.FromAssembly(development ? "ErrorStacked.html" : "Error.html"))
                                        .Build();
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return Content.Handle(request);
        }

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Content.GetContent(request);
        }

        public TemplateModel Render(ErrorModel error, ContentInfo details)
        {
            return new TemplateModel(error.Request, error.Handler, details, ErrorRenderer.Render(error));
        }

        public IResponseBuilder Render(TemplateModel model)
        {
            return model.Request.Respond()
                                .Content(Template.Render(model))
                                .Type(ContentType.TextHtml);
        }

        #endregion

    }

}
