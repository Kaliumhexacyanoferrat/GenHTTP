using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Protocol;

namespace GenHTTP.Api.Modules.Templating
{

    public class PageModel : IBaseModel
    {

        #region Get-/Setters

        public string? Title { get; set; }

        public IRequest Request { get; }
        
        #endregion

        #region Functionality

        public PageModel(IRequest request)
        {
            Request = request;
        }

        #endregion

    }

}
