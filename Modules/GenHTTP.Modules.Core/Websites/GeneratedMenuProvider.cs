using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

namespace GenHTTP.Modules.Core.Websites
{

    public class GeneratedMenuProvider : IMenuProvider
    {

        #region Get-/Setters

        private IRouter Router { get; }

        #endregion

        #region Initialization 

        public GeneratedMenuProvider(IRouter router)
        {
            Router = router;
        }

        #endregion

        #region Functionality

        public List<ContentElement> GetMenu(IRequest request)
        {
            var elements = Router.GetContent(request, "");

            return new List<ContentElement>(elements);
        }
        
        #endregion

    }

}
