using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;
using GenHTTP.Api.Routing;

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

            var webPath = new WebPath(new List<string>() { path }, false);

            if (children != null)
            {
                childElements = new List<ContentElement>(children.Count);

                foreach (var child in children)
                {
                    var childParts = new List<string>(webPath.Parts);
                    childParts.Add(child.childPath);

                    var childPath = new WebPath(childParts, false);

                    childElements.Add(new ContentElement(childPath, child.childTitle, ContentType.TextHtml, null));
                }
            }

            _Menu.Add(new ContentElement(webPath, title, ContentType.TextHtml, childElements));

            return this;
        }

        public IMenuProvider Build()
        {
            return new StaticMenuProvider(_Menu);
        }

        #endregion

    }

}
