using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Webservices.Provider
{

    public sealed class ServiceResourceBuilder : IHandlerBuilder<ServiceResourceBuilder>
    {
        private object? _Instance;

        private IBuilder<SerializationRegistry>? _Formats;

        private IBuilder<InjectionRegistry>? _Injectors;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public ServiceResourceBuilder Type<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>() where T : new() => Instance(new T());

        public ServiceResourceBuilder Instance(object instance)
        {
            _Instance = instance;
            return this;
        }

        public ServiceResourceBuilder Formats(IBuilder<SerializationRegistry> registry)
        {
            _Formats = registry;
            return this;
        }

        public ServiceResourceBuilder Injectors(IBuilder<InjectionRegistry> registry)
        {
            _Injectors = registry;
            return this;
        }

        public ServiceResourceBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            var injectors = (_Injectors ?? Injection.Default()).Build();

            var instance = _Instance ?? throw new BuilderMissingPropertyException("instance");

            return Concerns.Chain(parent, _Concerns, (p) => new ServiceResourceRouter(p, instance, formats, injectors));
        }

        #endregion

    }

}
