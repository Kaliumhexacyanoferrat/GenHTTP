using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceBuilder : IHandlerBuilder
    {
        private object? _Instance;

        private IBuilder<SerializationRegistry>? _Formats;

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

        public IHandler Build(IHandler parent)
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            var instance = _Instance ?? throw new BuilderMissingPropertyException("instance");

            return new ResourceRouter(parent, instance, formats);
        }

        #endregion

    }

}
