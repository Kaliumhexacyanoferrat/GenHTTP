using System.Collections.Generic;
using System.Threading.Tasks;

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

        public ValueTask<List<ContentElement>> GetMenuAsync(IRequest request, IHandler handler) => new(Menu);

        #endregion

    }

}
