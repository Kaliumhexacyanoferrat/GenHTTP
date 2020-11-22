using System;
using System.Collections.Generic;
using System.Reflection;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Content.Templating;
using GenHTTP.Api.Content.IO;

namespace GenHTTP.Modules.Razor.Providers
{

    public class RazorRendererBuilder<T> : IBuilder<IRenderer<T>>, IRazorConfigurationBuilder<RazorRendererBuilder<T>> where T : class, IBaseModel
    {
        private IResource? _TemplateProvider;

        private readonly List<Assembly> _AdditionalAssemblies = new();

        private readonly List<string> _AditionalUsings = new();

        #region Functionality

        public RazorRendererBuilder<T> TemplateProvider(IResource templateProvider)
        {
            _TemplateProvider = templateProvider;
            return this;
        }

        public RazorRendererBuilder<T> AddUsing(string nameSpace)
        {
            _AditionalUsings.Add(nameSpace);
            return this;
        }

        internal RazorRendererBuilder<T> AddUsings(IEnumerable<string> nameSpaces)
        {
            _AditionalUsings.AddRange(nameSpaces);
            return this;
        }

        public RazorRendererBuilder<T> AddAssemblyReference<Type>()
        {
            _AdditionalAssemblies.Add(typeof(Type).Assembly);
            return this;
        }

        public RazorRendererBuilder<T> AddAssemblyReference(Type type)
        {
            _AdditionalAssemblies.Add(type.Assembly);
            return this;
        }

        public RazorRendererBuilder<T> AddAssemblyReference(Assembly assembly)
        {
            _AdditionalAssemblies.Add(assembly);
            return this;
        }

        public RazorRendererBuilder<T> AddAssemblyReference(string assembly)
        {
            _AdditionalAssemblies.Add(Assembly.Load(assembly));
            return this;
        }

        internal RazorRendererBuilder<T> AddAssemblyReferences(List<Assembly> assemblies)
        {
            _AdditionalAssemblies.AddRange(assemblies);
            return this;
        }

        public IRenderer<T> Build()
        {
            if (_TemplateProvider is null)
            {
                throw new BuilderMissingPropertyException("Template Provider");
            }

            return new RazorRenderer<T>(_TemplateProvider, _AdditionalAssemblies, _AditionalUsings);
        }

        #endregion

    }

}
