using GenHTTP.Api.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace GenHTTP.Api.Modules.Templating
{

    public class TemplateModel : IBaseModel
    {

        #region Get-/Setters

        public string Title { get; }

        public string Content { get; }

        public IRequest Request { get; }

        #endregion

        #region Initialization

        public TemplateModel(IRequest request, string title, string content)
        {
            Title = title;
            Content = content;

            Request = request;
        }

        #endregion

    }

}

