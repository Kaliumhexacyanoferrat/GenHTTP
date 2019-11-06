using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core.General
{

    public class PageProvider : ContentProviderBase
    {

        #region Get-/Setters

        public string? Title { get; }

        public IResourceProvider Content { get; }

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
            if (request.HasType(RequestMethod.HEAD, RequestMethod.GET, RequestMethod.POST))
            {
                var templateModel = new TemplateModel(request, Title ?? "Untitled Page", Content.GetResourceAsString());

                return request.Respond()
                              .Content(templateModel);
            }

            return request.Respond(ResponseStatus.MethodNotAllowed);
        }

        #endregion

    }

}
