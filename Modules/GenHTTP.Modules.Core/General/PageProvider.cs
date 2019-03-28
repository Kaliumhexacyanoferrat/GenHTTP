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

        public string Content { get; }

        #endregion

        #region Initialization

        public PageProvider(string? title, string content)
        {
            Title = title;
            Content = content;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            var templateModel = new TemplateModel(request, response, Title ?? "Untitled Page", Content);

            response.Send(templateModel, request);
        }

        #endregion

    }

}
