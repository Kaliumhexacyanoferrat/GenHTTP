using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Modules;

namespace GenHTTP.Modules.Core.General
{

    public class PageProviderBuilder : IContentBuilder
    {
        protected string? _Title;
        protected string? _Content;

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Title = title;
            return this;
        }

        public PageProviderBuilder Content(string content)
        {
            _Content = content;
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
