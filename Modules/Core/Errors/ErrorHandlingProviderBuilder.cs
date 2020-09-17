using GenHTTP.Api.Content;
using System;

namespace GenHTTP.Modules.Core.Errors
{

    public class ErrorHandlingProviderBuilder : IConcernBuilder
    {

        #region Functionality

        public IConcern Build(IHandler parent, Func<IHandler, IHandler> contentFactory)
        {
            return new ErrorHandlingProvider(parent, contentFactory);
        }


        #endregion

    }

}
