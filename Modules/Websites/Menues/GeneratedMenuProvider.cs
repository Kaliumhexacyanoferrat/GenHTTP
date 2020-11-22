using System;
using System.Collections.Generic;
using System.Linq;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Menues
{

    public sealed class GeneratedMenuProvider : IMenuProvider
    {

        #region Get-/Setters

        private Func<IRequest, IHandler, IEnumerable<ContentElement>> Provider { get; }

        #endregion

        #region Initialization 

        public GeneratedMenuProvider(Func<IRequest, IHandler, IEnumerable<ContentElement>> provider)
        {
            Provider = provider;
        }

        #endregion

        #region Functionality

        public List<ContentElement> GetMenu(IRequest request, IHandler handler)
        {
            return new List<ContentElement>(Provider(request, handler).Where(c => c.ContentType == ContentType.TextHtml));
        }

        #endregion

    }

}
