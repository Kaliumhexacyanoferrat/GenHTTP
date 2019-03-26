using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Templating
{
    
    public class TemplatedContent<T> : TemplateBased<T>, IContentProvider where T : TemplatedContentViewModel
    {

        #region Get-/Setters

        public Func<IHttpRequest, IHttpResponse, T> ViewModelProvider { get; }
        
        #endregion

        #region Initialization

        public TemplatedContent(string template, Func<IHttpRequest, IHttpResponse, T> viewModelProvider) : base(template)
        {
            ViewModelProvider = viewModelProvider;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            var viewModel = ViewModelProvider(request, response);

            var content = Render(viewModel);

            var page = request.Routing?.Router.GetPage(request, response) 
                ?? throw new InvalidOperationException("Routing context is not set");
            
            page.Title = viewModel.Title ?? "No Title";
            page.Content = content;

            using (var stream = page.GetStream())
            {
                response.Send(stream);
            }
        }
        
        #endregion

    }

}
