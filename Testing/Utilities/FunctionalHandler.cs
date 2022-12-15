using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Testing.Acceptance.Utilities
{

    public sealed class FunctionalHandler : IHandler
    {
        private readonly Func<IRequest, IAsyncEnumerable<ContentElement>>? _ContentProvider;

        private readonly Func<IRequest, IResponse?>? _ResponseProvider;

        private IHandler? _Parent;

        #region Get-/Setters

        public IHandler Parent
        {
            get { return _Parent ?? throw new InvalidOperationException(); }
            set { _Parent = value; }
        }

        #endregion

        #region Initialization

        public FunctionalHandler(Func<IRequest, IAsyncEnumerable<ContentElement>>? contentProvider = null, Func<IRequest, IResponse?>? responseProvider = null)
        {
            _ContentProvider = contentProvider;
            _ResponseProvider = responseProvider;
        }

        #endregion

        #region Functionality

        public ValueTask PrepareAsync() => ValueTask.CompletedTask;

        public IAsyncEnumerable<ContentElement> GetContentAsync(IRequest request) => (_ContentProvider is not null) ? _ContentProvider(request) : AsyncEnumerable.Empty<ContentElement>();

        public ValueTask<IResponse?> HandleAsync(IRequest request) => new((_ResponseProvider is not null) ? _ResponseProvider(request) : null);

        #endregion

    }

}
