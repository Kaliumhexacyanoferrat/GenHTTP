using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class GeneratedMenuProvider : IMenuProvider
    {

        #region Get-/Setters

        private IHandler Handler { get; }

        #endregion

        #region Initialization 

        public GeneratedMenuProvider(IHandler handler)
        {
            Handler = handler;
        }

        #endregion

        #region Functionality

        public List<ContentElement> GetMenu(IRequest request)
        {
            var elements = Handler.GetContent(request);

            return new List<ContentElement>(elements);
        }

        #endregion

    }

}
