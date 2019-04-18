using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public abstract class ContentProviderBase : IContentProvider
    {

        #region Get-/Setters

        public ResponseModification? Modification { get; }

        #endregion

        #region Initialization

        protected ContentProviderBase(ResponseModification? modification)
        {
            Modification = modification;
        }

        #endregion

        #region Functionality
        
        public IResponseBuilder Handle(IRequest request)
        {
            var response = HandleInternal(request);

            Modification?.Invoke(response);

            return response;
        }

        protected abstract IResponseBuilder HandleInternal(IRequest request);

        #endregion

    }

}
