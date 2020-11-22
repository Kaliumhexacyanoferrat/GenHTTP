﻿using System.Collections.Generic;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Scriban.Providers
{

    public sealed class ScribanPageProviderBuilder<T> : IHandlerBuilder<ScribanPageProviderBuilder<T>>, IContentInfoBuilder<ScribanPageProviderBuilder<T>> where T : PageModel
    {
        private IResource? _TemplateProvider;

        private ModelProvider<T>? _ModelProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

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

            return Concerns.Chain(parent, _Concerns, (p) => new ScribanPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Info.Build()));
        }

        #endregion

    }

}
