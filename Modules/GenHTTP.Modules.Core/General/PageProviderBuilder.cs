using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.General
{

    public class PageProviderBuilder : IContentBuilder
    {
        protected IResourceProvider? _Content;
        protected string? _Title;

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public PageProviderBuilder Content(IResourceProvider templateProvider)
        {
            _Content = templateProvider;
            return this;
        }

        public IContentProvider Build()
        {
            if (_Content == null)
            {
                throw new BuilderMissingPropertyException("Content");
            }

            return new PageProvider(_Title, _Content);
        }

        #endregion

    }

}
