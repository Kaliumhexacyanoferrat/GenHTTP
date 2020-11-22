using System;

using GenHTTP.Api.Content;

namespace GenHTTP.Modules.ErrorHandling.Provider
{

    public sealed class ErrorHandlingProviderBuilder : IConcernBuilder
    {

        #region Functionality

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new ErrorHandlingProvider(parent, contentFactory);
        }


        #endregion

    }

}
