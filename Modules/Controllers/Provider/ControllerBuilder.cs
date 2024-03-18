using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Formatters;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Controllers.Provider
{

    public sealed class ControllerBuilder : IHandlerBuilder<ControllerBuilder>
    {
        private IBuilder<SerializationRegistry>? _Serializers;

        private IBuilder<InjectionRegistry>? _Injection;

        private IBuilder<FormatterRegistry>? _Formatters;

        private readonly List<IConcernBuilder> _Concerns = new();

        private object? _Instance;

        #region Functionality

        public ControllerBuilder Serializers(IBuilder<SerializationRegistry> registry)
        {
            _Serializers = registry;
            return this;
        }

        public ControllerBuilder Injectors(IBuilder<InjectionRegistry> registry)
        {
            _Injection = registry;
            return this;
        }

        public ControllerBuilder Formatters(IBuilder<FormatterRegistry> registry)
        {
            _Formatters = registry;
            return this;
        }

        public ControllerBuilder Type<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new()
        {
            _Instance = new T();
            return this;
        }

        public ControllerBuilder Instance(object instance)
        {
            _Instance = instance;
            return this;
        }

        public ControllerBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var serializers = (_Serializers ?? Serialization.Default()).Build();

            var injectors = (_Injection ?? Injection.Default()).Build();

            var formatters = (_Formatters ?? Formatting.Default()).Build();

            var instance = _Instance ?? throw new BuilderMissingPropertyException("Instance or Type");

            return Concerns.Chain(parent, _Concerns, (p) => new ControllerHandler(p, instance, serializers, injectors, formatters));
        }

        #endregion

    }

}
