using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Protocol;
using System;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PageProviderBuilder : 
        IHandlerBuilder<PageProviderBuilder>, 
        IContentInfoBuilder<PageProviderBuilder>, 
        IPageAdditionBuilder<PageProviderBuilder>,
        IResponseModification<PageProviderBuilder>
    {
        private IResource? _Content;

        private readonly ContentInfoBuilder _Info = new();

        private readonly PageAdditionBuilder _Additions = new();

        private readonly ResponseModificationBuilder _Modifications = new();

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public PageProviderBuilder Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public PageProviderBuilder Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public PageProviderBuilder Content(IResource templateProvider)
        {
            _Content = templateProvider;
            return this;
        }

        public PageProviderBuilder AddScript(string path, bool asynchronous = false)
        {
            _Additions.AddScript(path, asynchronous);
            return this;
        }

        public PageProviderBuilder AddStyle(string path)
        {
            _Additions.AddStyle(path);
            return this;
        }

        public PageProviderBuilder Status(ResponseStatus status)
        {
            _Modifications.Status(status);
            return this;
        }

        public PageProviderBuilder Status(int status, string reason)
        {
            _Modifications.Status(status, reason);
            return this;
        }

        public PageProviderBuilder Header(string key, string value)
        {
            _Modifications.Header(key, value);
            return this;
        }

        public PageProviderBuilder Expires(DateTime expiryDate)
        {
            _Modifications.Expires(expiryDate);
            return this;
        }

        public PageProviderBuilder Modified(DateTime modificationDate)
        {
            _Modifications.Modified(modificationDate);
            return this;
        }

        public PageProviderBuilder Cookie(Cookie cookie)
        {
            _Modifications.Cookie(cookie);
            return this;
        }

        public PageProviderBuilder Type(FlexibleContentType contentType)
        {
            _Modifications.Type(contentType);
            return this;
        }

        public PageProviderBuilder Encoding(string encoding)
        {
            _Modifications.Encoding(encoding);
            return this;
        }

        public PageProviderBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_Content is null)
            {
                throw new BuilderMissingPropertyException("Content");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new PageProvider(p, _Info.Build(), _Additions.Build(), _Modifications.Build(), _Content)); ;
        }

        #endregion

    }

}
