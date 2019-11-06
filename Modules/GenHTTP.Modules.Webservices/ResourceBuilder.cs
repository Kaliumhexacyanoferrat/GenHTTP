using GenHTTP.Api.Infrastructure;

using GenHTTP.Modules.Core.General;

namespace GenHTTP.Modules.Webservices
{

    public class ResourceBuilder : RouterBuilderBase<ResourceBuilder, ResourceRouter>
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

        public override ResourceRouter Build()
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            var instance = _Instance ?? throw new BuilderMissingPropertyException("instance");

            return new ResourceRouter(instance, formats, _Template, _ErrorHandler);
        }

        #endregion

    }

}
