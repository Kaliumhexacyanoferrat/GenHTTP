using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.IO;
using GenHTTP.Modules.Placeholders;

namespace GenHTTP.Engine.Infrastructure
{

    internal sealed class CoreRouter : IHandler, IErrorHandler, IPageRenderer
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

        public async ValueTask<IResponse?> HandleAsync(IRequest request)
        {
            return await Content.HandleAsync(request).ConfigureAwait(false);
        }

        public async ValueTask PrepareAsync()
        {
            await Content.PrepareAsync();

            await Template.PrepareAsync();

            await ErrorRenderer.PrepareAsync();
        }

        ValueTask<ulong> IRenderer<ErrorModel>.CalculateChecksumAsync() => ErrorRenderer.CalculateChecksumAsync();

        ValueTask<ulong> IRenderer<TemplateModel>.CalculateChecksumAsync() => Template.CalculateChecksumAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            return Content.GetContent(request);
        }

        public async ValueTask<string> RenderAsync(ErrorModel model)
        {
            return await ErrorRenderer.RenderAsync(model).ConfigureAwait(false);
        }

        public async ValueTask<string> RenderAsync(TemplateModel model)
        {
            return await Template.RenderAsync(model);
        }

        #endregion

    }

}
