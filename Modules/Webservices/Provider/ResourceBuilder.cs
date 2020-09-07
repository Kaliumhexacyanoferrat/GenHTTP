using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Providers;

namespace GenHTTP.Modules.Webservices.Provider
{

    public class ResourceBuilder : IHandlerBuilder<ResourceBuilder>
    {
        private object? _Instance;

        private IBuilder<SerializationRegistry>? _Formats;

        private readonly List<IConcernBuilder> _Concerns = new List<IConcernBuilder>();

        #region Functionality

        public ResourceBuilder Type<T>() where T : new() => Instance(new T());

        public ResourceBuilder Instance(object instance)
        {
            _Instance = instance;
            return this;
        }

        public ResourceBuilder Formats(IBuilder<SerializationRegistry> registry)
        {
            _Formats = registry;
            return this;
        }

        public ResourceBuilder Add(IConcernBuilder concern)
        {
            _Concerns.Add(concern);
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            var instance = _Instance ?? throw new BuilderMissingPropertyException("instance");

            return Concerns.Chain(parent, _Concerns, (p) => new ResourceRouter(p, instance, formats));
        }

        #endregion

    }

}
