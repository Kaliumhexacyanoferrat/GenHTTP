using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Content.Pages;

namespace GenHTTP.Content.Templating
{

    internal class TemplatedPage : TemplateBased<TemplatedTemplateViewModel>, IContentPage
    {

        #region Get-/Setters

        public string Title
        {
            get => ViewModel.Title;
            set => ViewModel.Title = value;
        }

        public string Content
        {
            get => ViewModel.Content;
            set => ViewModel.Content = value;
        }

        internal TemplatedTemplateViewModel ViewModel { get; }

        #endregion

        #region Initialization

        internal TemplatedPage(string template, TemplatedTemplateViewModel viewModel) : base(template)
        {
            ViewModel = viewModel;
        }

        #endregion

        #region Functionality

        public Stream GetStream()
        {
            var result = Render(ViewModel);

            return new MemoryStream(Encoding.UTF8.GetBytes(result));
        }

        #endregion

    }

}
