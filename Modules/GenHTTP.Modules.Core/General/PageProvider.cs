using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Templating;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Core.Templating;

namespace GenHTTP.Modules.Core.General
{

    public class PageProvider : IContentProvider
    {

        #region Get-/Setters

        public string? Title { get; }

        public IResourceProvider Content { get; }

        #endregion

        #region Initialization

        public PageProvider(string? title, IResourceProvider content)
        {
            Title = title;
            Content = content;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Handle(IRequest request)
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
