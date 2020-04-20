using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.Websites
{

    public class StaticMenuBuilder : IBuilder<IMenuProvider>
    {

        #region Get-/Setters

        private readonly List<ContentElement> _Menu = new List<ContentElement>();

        #endregion

        #region Functionality

        public StaticMenuBuilder Add(string path, string title, List<(string childPath, string childTitle)>? children = null)
        {
            List<ContentElement>? childElements = null;

            if (children != null)
            {
                childElements = new List<ContentElement>(children.Count);

                foreach (var child in children)
                {                 
                    childElements.Add(new ContentElement($"{path}{child.childPath}", child.childTitle, ContentType.TextHtml, null));
                }
            }

            _Menu.Add(new ContentElement(path, title, ContentType.TextHtml, childElements));

            return this;
        }

        public IMenuProvider Build()
        {
            return new StaticMenuProvider(_Menu);
        }

        #endregion

    }

}
