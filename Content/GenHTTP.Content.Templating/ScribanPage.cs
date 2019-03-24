using GenHTTP.Api.Content.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GenHTTP.Content.Templating
{

    internal class ScribanPage : ScribanBased<ScribanTemplateViewModel>, IContentPage
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

        internal ScribanTemplateViewModel ViewModel { get; }

        #endregion

        #region Initialization

        internal ScribanPage(string template, ScribanTemplateViewModel viewModel) : base(template)
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
