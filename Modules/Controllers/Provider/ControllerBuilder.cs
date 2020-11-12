using GenHTTP.Api.Content;
using GenHTTP.Api.Infrastructure;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Modules.Conversion;
using GenHTTP.Modules.Conversion.Providers;

namespace GenHTTP.Modules.Controllers.Provider
{

    public class ControllerBuilder<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T> : IHandlerBuilder where T : new()
    {
        private IBuilder<SerializationRegistry>? _Formats;

        #region Functionality

        public ControllerBuilder<T> Formats(IBuilder<SerializationRegistry> registry)
        {
            _Formats = registry;
            return this;
        }

        public IHandler Build(IHandler parent)
        {
            var formats = (_Formats ?? Serialization.Default()).Build();

            return new ControllerHandler<T>(parent, formats);
        }

        #endregion

    }

}
