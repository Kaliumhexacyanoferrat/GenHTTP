using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Controllers.Provider
{

    public sealed class ControllerBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IHandlerBuilder<ControllerBuilder<T>> where T : new()
    {
        private IBuilder<SerializationRegistry>? _Formats;

        private IBuilder<InjectionRegistry>? _Injection;

        private readonly List<IConcernBuilder> _Concerns = new();

        #region Functionality

        public ControllerBuilder<T> Formats(IBuilder<SerializationRegistry> registry)
        {
            _Formats = registry;
            return this;
        }

        public ControllerBuilder<T> Injectors(IBuilder<InjectionRegistry>? registry)
        {
            _Injection = registry;
            return this;
        }

        public ControllerBuilder<T> Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            var injectors = (_Injection ?? Injection.Default()).Build();

            return Concerns.Chain(parent, _Concerns, (p) => new ControllerHandler<T>(p, formats, injectors));
        }

        #endregion

    }

}
