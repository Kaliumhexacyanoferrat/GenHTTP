using System;
using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Websites;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Websites.Menues
{

    public sealed class StaticMenuBuilder : IBuilder<IMenuProvider>
    {

        #region Get-/Setters

        private readonly List<ContentElement> _Menu = new();

        #endregion

        #region Functionality

        public StaticMenuBuilder Add(string path, string title, List<(string childPath, string childTitle)>? children = null)
        {
            List<ContentElement>? childElements = null;

            if (children is not null)
            {
                childElements = new List<ContentElement>(children.Count);

                foreach (var child in children)
                {
                    var childInfo = ContentInfo.Create()
                                               .Title(child.childTitle)
                                               .Build();

                    childElements.Add(new ContentElement($"{path}{child.childPath}", childInfo, ContentType.TextHtml, null));
                }
            }

            var info = ContentInfo.Create()
                                  .Title(title)
                                  .Build();

            _Menu.Add(new ContentElement(path, info, ContentType.TextHtml, childElements));

            return this;
        }

        public IMenuProvider Build()
        {
            return new StaticMenuProvider(_Menu);
        }

        #endregion

    }

}
