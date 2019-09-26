using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Modules.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class StaticMenuProvider : IMenuProvider
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

        public List<ContentElement> GetMenu(IRequest request) => Menu;

        #endregion

    }

}
