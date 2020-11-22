using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Menues
{

    public sealed class StaticMenuProvider : IMenuProvider
    {

        #region Get-/Setters

        private List<ContentElement> Menu { get; }

        #endregion

        #region Initialization

        public StaticMenuProvider(List<ContentElement> menu)
        {
            Menu = menu;
        }

        #endregion

        #region Functionality

        public List<ContentElement> GetMenu(IRequest request, IHandler handler) => Menu;

        #endregion

    }

}
