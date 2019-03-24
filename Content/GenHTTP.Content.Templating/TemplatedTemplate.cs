using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content.Pages;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Templating
{
     
    public class TemplatedTemplate<T> : ITemplateProvider where T : TemplatedTemplateViewModel
    {

        #region Get-/Setters

        public string Template { get; }

        public Func<IHttpRequest, IHttpResponse, T> ViewModelProvider { get; }

        #endregion

        #region Initialization

        public TemplatedTemplate(string template, Func<IHttpRequest, IHttpResponse, T> viewModelProvider)
        {
            Template = template;
            ViewModelProvider = viewModelProvider;
        }

        #endregion

        #region Functionality

        public IContentPage GetPage(IHttpRequest request, IHttpResponse response)
        {
            var viewModel = ViewModelProvider(request, response);

            return new TemplatedPage(Template, viewModel);
        }

        #endregion

    }

}
