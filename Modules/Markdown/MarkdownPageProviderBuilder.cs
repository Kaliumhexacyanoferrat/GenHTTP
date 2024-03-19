using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Markdown
{

    public sealed class MarkdownPageProviderBuilder<T> : IHandlerBuilder<MarkdownPageProviderBuilder<T>>, 
        IContentInfoBuilder<MarkdownPageProviderBuilder<T>>, 
        IPageAdditionBuilder<MarkdownPageProviderBuilder<T>>,
        IResponseModification<MarkdownPageProviderBuilder<T>> where T : class, IModel
    {
        private IResource? _FileProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly PageAdditionBuilder _Additions = new();

        private readonly ResponseModificationBuilder _Modifications = new();

        #region Functionality

        public MarkdownPageProviderBuilder<T> File(IResource fileProvider)
        {
            _FileProvider = fileProvider;
            return this;
        }

        public MarkdownPageProviderBuilder<T> Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public MarkdownPageProviderBuilder<T> AddScript(string path, bool asynchronous = false)
        {
            _Additions.AddScript(path, asynchronous);
            return this;
        }

        public MarkdownPageProviderBuilder<T> AddStyle(string path)
        {
            _Additions.AddStyle(path);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Status(ResponseStatus status)
        {
            _Modifications.Status(status);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Status(int status, string reason)
        {
            _Modifications.Status(status, reason);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Header(string key, string value)
        {
            _Modifications.Header(key, value);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Expires(DateTime expiryDate)
        {
            _Modifications.Expires(expiryDate);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Modified(DateTime modificationDate)
        {
            _Modifications.Modified(modificationDate);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Cookie(Cookie cookie)
        {
            _Modifications.Cookie(cookie);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Type(FlexibleContentType contentType)
        {
            _Modifications.Type(contentType);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Encoding(string encoding)
        {
            _Modifications.Encoding(encoding);
            return this;
        }

        public MarkdownPageProviderBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_FileProvider is null)
            {
                throw new BuilderMissingPropertyException("File Provider");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new MarkdownPageProvider<T>(p, _FileProvider, _Info.Build(), _Additions.Build(), _Modifications.Build()));
        }

        #endregion

    }

}