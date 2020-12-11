using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Content.IO;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Razor.Providers
{

    public sealed class RazorPageProviderBuilder<T> : IHandlerBuilder<RazorPageProviderBuilder<T>>, IContentInfoBuilder<RazorPageProviderBuilder<T>>, IRazorConfigurationBuilder<RazorPageProviderBuilder<T>> where T : PageModel
    {
        private IResource? _TemplateProvider;

        private ModelProvider<T>? _ModelProvider;

        private readonly List<IConcernBuilder> _Concerns = new();

        private readonly ContentInfoBuilder _Info = new();

        private readonly List<Assembly> _AdditionalAssemblies = new();

        private readonly List<string> _AditionalUsings = new();

        #region Functionality

        public RazorPageProviderBuilder<T> Template(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public RazorPageProviderBuilder<T> Model(ModelProvider<T> modelProvider)
        {
            _ModelProvider = modelProvider;
            return this;
        }

        public RazorPageProviderBuilder<T> Model(Func<IRequest, IHandler, T> modelProvider)
        {
            _ModelProvider = (r, h) => new ValueTask<T>(modelProvider(r, h));
            return this;
        }

        public RazorPageProviderBuilder<T> Title(string title)
        {
            _Info.Title(title);
            return this;
        }

        public RazorPageProviderBuilder<T> Description(string description)
        {
            _Info.Description(description);
            return this;
        }

        public RazorPageProviderBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public RazorPageProviderBuilder<T> AddUsing(string nameSpace)
        {
            _AditionalUsings.Add(nameSpace);
            return this;
        }

        public RazorPageProviderBuilder<T> AddAssemblyReference<Type>()
        {
            _AdditionalAssemblies.Add(typeof(Type).Assembly);
            return this;
        }

        public RazorPageProviderBuilder<T> AddAssemblyReference(Type type)
        {
            _AdditionalAssemblies.Add(type.Assembly);
            return this;
        }

        public RazorPageProviderBuilder<T> AddAssemblyReference(Assembly assembly)
        {
            _AdditionalAssemblies.Add(assembly);
            return this;
        }

        public RazorPageProviderBuilder<T> AddAssemblyReference(string assembly)
        {
            _AdditionalAssemblies.Add(Assembly.Load(assembly));
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

            return Concerns.Chain(parent, _Concerns, (p) => new RazorPageProvider<T>(p, _TemplateProvider, _ModelProvider, _Info.Build(), _AdditionalAssemblies, _AditionalUsings));
        }

        #endregion

    }

}
