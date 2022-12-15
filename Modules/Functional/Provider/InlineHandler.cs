using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using GenHTTP.Modules.Reflection.Injectors;

namespace GenHTTP.Modules.Functional.Provider
{

    public class InlineHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private MethodCollection Methods { get; }

        private ResponseProvider ResponseProvider { get; }

        #endregion

        #region Initialization

        public InlineHandler(IHandler parent, List<InlineFunction> functions, SerializationRegistry formats, InjectionRegistry injection)
        {
            Parent = parent;

            ResponseProvider = new(formats);

            Methods = new(this, AnalyzeMethods(functions, formats, injection));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(List<InlineFunction> functions, SerializationRegistry formats, InjectionRegistry injection)
        {
            foreach (var function in functions)
            {
                var path = PathArguments.Route(function.Path);

                var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

                yield return (parent) => new MethodHandler(parent, function.Delegate.Method, path, () => target, function.Configuration, ResponseProvider.GetResponse, formats, injection);
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
