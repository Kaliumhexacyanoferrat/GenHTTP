using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public abstract class ContentProviderBase : IContentProvider
    {

        #region Get-/Setters

        public ResponseModification? Modification { get; }

        public abstract string? Title { get; }

        public abstract FlexibleContentType? ContentType { get; }

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
