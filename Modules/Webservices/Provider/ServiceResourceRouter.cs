using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Webservices.Provider
{

    public sealed class ServiceResourceRouter : IHandler
    {

        #region Get-/Setters

        private MethodCollection Methods { get; }

        public IHandler Parent { get; }

        public ResponseProvider ResponseProvider { get; }

        public object Instance { get; }

        #endregion

        #region Initialization

        public ServiceResourceRouter(IHandler parent, object instance, SerializationRegistry serialization, InjectionRegistry injection)
        {
            Parent = parent;

            Instance = instance;

            ResponseProvider = new(serialization);

            Methods = new(this, AnalyzeMethods(instance.GetType(), serialization, injection));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(Type type, SerializationRegistry serialization, InjectionRegistry injection)
        {
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attribute = method.GetCustomAttribute<ResourceMethodAttribute>(true);

                if (attribute is not null)
                {
                    var path = PathArguments.Route(attribute.Path);

                    yield return (parent) => new MethodHandler(parent, method, path, () => Instance, attribute, ResponseProvider.GetResponse, serialization, injection);
                }
            }
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Methods.PrepareAsync();

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => Methods.GetContentAsync(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

        #endregion

    }

}
