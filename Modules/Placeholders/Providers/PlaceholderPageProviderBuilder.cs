using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Placeholders.Providers
{

    public sealed class PlaceholderPageProviderBuilder<T> :
        IHandlerBuilder<PlaceholderPageProviderBuilder<T>>,
        IContentInfoBuilder<PlaceholderPageProviderBuilder<T>>, 
        IPageAdditionBuilder<PlaceholderPageProviderBuilder<T>>,
        IResponseModification<PlaceholderPageProviderBuilder<T>> where T : class, IModel
    {
        private IResource? _TemplateProvider;

        private ModelProvider<T>? _ModelProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly PageAdditionBuilder _Additions = new();

        private readonly ResponseModificationBuilder _Modifications = new();

        #region Functionality

        public PlaceholderPageProviderBuilder<T> Template(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Model(Func<IRequest, IHandler, T> modelProvider)
        {
            _ModelProvider = (r, h) => new ValueTask<T>(modelProvider(r, h));
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> AddScript(string path, bool asynchronous = false)
        {
            _Additions.AddScript(path, asynchronous);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> AddStyle(string path)
        {
            _Additions.AddStyle(path);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Status(ResponseStatus status)
        {
            _Modifications.Status(status);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Status(int status, string reason)
        {
            _Modifications.Status(status, reason);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Header(string key, string value)
        {
            _Modifications.Header(key, value);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Expires(DateTime expiryDate)
        {
            _Modifications.Expires(expiryDate);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Modified(DateTime modificationDate)
        {
            _Modifications.Modified(modificationDate);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Cookie(Cookie cookie)
        {
            _Modifications.Cookie(cookie);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Type(FlexibleContentType contentType)
        {
            _Modifications.Type(contentType);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Encoding(string encoding)
        {
            _Modifications.Encoding(encoding);
            return this;
        }

        public PlaceholderPageProviderBuilder<T> Add(IConcernBuilder concern)
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

            return Concerns.Chain(parent, _Concerns, (p) => new PlaceholderPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Info.Build(), _Additions.Build(), _Modifications.Build()));
        }


        #endregion

    }

}
