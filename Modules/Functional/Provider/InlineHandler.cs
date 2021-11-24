using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;

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

        public InlineHandler(IHandler parent, List<InlineFunction> functions, SerializationRegistry formats)
        {
            Parent = parent;

            ResponseProvider = new(formats);

            Methods = new(this, AnalyzeMethods(functions, formats));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(List<InlineFunction> functions, SerializationRegistry formats)
        {
            foreach (var function in functions)
            {
                var path = PathArguments.Route(function.Path);

                var target = function.Delegate.Target ?? throw new InvalidOperationException("Delegate target must not be null");

                yield return (parent) => new MethodHandler(parent, function.Delegate.Method, path, () => target, function.Configuration, ResponseProvider.GetResponse, formats);
            }
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Methods.PrepareAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request) => Methods.GetContent(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Methods.HandleAsync(request);

        #endregion

    }

}
