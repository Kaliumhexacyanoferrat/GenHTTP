using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Content.Templating
{
    
    public class ScribanContent<T> : ScribanBased<T>, IContentProvider where T : ScribanContentViewModel
    {

        #region Get-/Setters

        public Func<IHttpRequest, T> ViewModelProvider { get; }
        
        #endregion

        #region Initialization

        public ScribanContent(string template, Func<IHttpRequest, T> viewModelProvider) : base(template)
        {
            ViewModelProvider = viewModelProvider;
        }

        #endregion

        #region Functionality

        public void Handle(IHttpRequest request, IHttpResponse response)
        {
            var viewModel = ViewModelProvider(request);

            var content = Render(viewModel);

            var page = request.Routing.Router.GetPage(request, response);

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
