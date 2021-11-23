using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Conversion.Providers;
using GenHTTP.Modules.Reflection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GenHTTP.Modules.Functional.Provider
{

    public class InlineHandler : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private MethodCollection Provider { get; }

        #endregion

        #region Initialization

        public InlineHandler(IHandler parent, List<InlineFunction> functions, SerializationRegistry formats)
        {
            Parent = parent;

            Provider = new(this, AnalyzeMethods(functions, formats));
        }

        private IEnumerable<Func<IHandler, MethodHandler>> AnalyzeMethods(List<InlineFunction> functions, SerializationRegistry formats)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => Provider.PrepareAsync();

        public IEnumerable<ContentElement> GetContent(IRequest request) => Provider.GetContent(request);

        public ValueTask<IResponse?> HandleAsync(IRequest request) => Provider.HandleAsync(request);

        #endregion

    }

}
