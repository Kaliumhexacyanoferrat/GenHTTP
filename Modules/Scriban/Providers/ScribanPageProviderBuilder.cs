﻿using System;
using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Scriban.Providers
{

    public sealed class ScribanPageProviderBuilder<T> : IHandlerBuilder<ScribanPageProviderBuilder<T>>, 
        IContentInfoBuilder<ScribanPageProviderBuilder<T>>, 
        IPageAdditionBuilder<ScribanPageProviderBuilder<T>>,
        IResponseModification<ScribanPageProviderBuilder<T>> where T : class, IModel
    {
        private IResource? _TemplateProvider;

        private ModelProvider<T>? _ModelProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly PageAdditionBuilder _Additions = new();

        private readonly ResponseModificationBuilder _Modifications = new();

        #region Functionality

        public ScribanPageProviderBuilder<T> Template(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public ScribanPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public ScribanPageProviderBuilder<T> Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public ScribanPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public ScribanPageProviderBuilder<T> AddScript(string path, bool asynchronous = false)
        {
            _Additions.AddScript(path, asynchronous);
            return this;
        }

        public ScribanPageProviderBuilder<T> AddStyle(string path)
        {
            _Additions.AddStyle(path);
            return this;
        }

        public ScribanPageProviderBuilder<T> Status(ResponseStatus status)
        {
            _Modifications.Status(status);
            return this;
        }

        public ScribanPageProviderBuilder<T> Status(int status, string reason)
        {
            _Modifications.Status(status, reason);
            return this;
        }

        public ScribanPageProviderBuilder<T> Header(string key, string value)
        {
            _Modifications.Header(key, value);
            return this;
        }

        public ScribanPageProviderBuilder<T> Expires(DateTime expiryDate)
        {
            _Modifications.Expires(expiryDate);
            return this;
        }

        public ScribanPageProviderBuilder<T> Modified(DateTime modificationDate)
        {
            _Modifications.Modified(modificationDate);
            return this;
        }

        public ScribanPageProviderBuilder<T> Cookie(Cookie cookie)
        {
            _Modifications.Cookie(cookie);
            return this;
        }

        public ScribanPageProviderBuilder<T> Type(FlexibleContentType contentType)
        {
            _Modifications.Type(contentType);
            return this;
        }

        public ScribanPageProviderBuilder<T> Encoding(string encoding)
        {
            _Modifications.Encoding(encoding);
            return this;
        }

        public ScribanPageProviderBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            if (_TemplateProvider is null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            if (_ModelProvider is null)
            {
                throw new BuilderMissingPropertyException("Model Provider");
            }

            return Concerns.Chain(parent, _Concerns, (p) => new ScribanPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Info.Build(), _Additions.Build(), _Modifications.Build()));
        }

        #endregion

    }

}
