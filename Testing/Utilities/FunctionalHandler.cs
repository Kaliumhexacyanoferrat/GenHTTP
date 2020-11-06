using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities
{

    public class FunctionalHandler : IHandler
    {
        private readonly Func<IRequest, IEnumerable<ContentElement>>? _ContentProvider;

        private readonly Func<IRequest, IResponse?>? _ResponseProvider;

        #region Get-/Setters

        public IHandler Parent => throw new NotImplementedException();

        #endregion

        #region Initialization

        public FunctionalHandler(Func<IRequest, IEnumerable<ContentElement>>? contentProvider = null, Func<IRequest, IResponse?>? responseProvider = null)
        {
            _ContentProvider = contentProvider;
            _ResponseProvider = responseProvider;
        }

        #endregion

        #region Functionality

        public IEnumerable<ContentElement> GetContent(IRequest request) => (_ContentProvider != null) ? _ContentProvider(request) : Enumerable.Empty<ContentElement>();

        public ValueTask<IResponse?> HandleAsync(IRequest request) => new ValueTask<IResponse?>((_ResponseProvider != null) ? _ResponseProvider(request) : null);

        #endregion

    }

}
