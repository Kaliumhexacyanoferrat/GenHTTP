using System;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ErrorHandling.Provider
{

    public sealed class ErrorHandlingProviderBuilder<T> : IConcernBuilder where T : Exception
    {

        #region Get-/Setters

        private IErrorHandler<T> Handler { get; }

        #endregion

        #region Initialization

        public ErrorHandlingProviderBuilder(IErrorHandler<T> handler)
        {
            Handler = handler;
        }

        #endregion

        #region Functionality

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new ErrorHandlingProvider<T>(parent, contentFactory, Handler);
        }

        #endregion

    }

}
