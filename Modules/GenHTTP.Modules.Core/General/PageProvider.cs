using System.Collections.Generic;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core.General
{

    public class PageProvider : ContentProviderBase
    {

        #region Get-/Setters

        public override string? Title { get; }

        public IResourceProvider Content { get; }

        public override FlexibleContentType? ContentType => new FlexibleContentType(Api.Protocol.ContentType.TextHtml);

        protected override HashSet<FlexibleRequestMethod>? SupportedMethods => _GET_POST;

        #endregion

        #region Initialization

        public PageProvider(string? title, IResourceProvider content, ResponseModification? mod) : base(mod)
        {
            Title = title;
            Content = content;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            var templateModel = new TemplateModel(request, Title ?? "Untitled Page", Content.GetResourceAsString());

            return request.Respond()
                          .Content(templateModel)
                          .Type(ContentType!.Value);
        }

        #endregion

    }

}
