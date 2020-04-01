using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.General
{

    public abstract class ContentProviderBase : IContentProvider
    {
        protected static readonly HashSet<FlexibleRequestMethod> _GET = new HashSet<FlexibleRequestMethod> { new FlexibleRequestMethod(RequestMethod.GET) };
        
        protected static readonly HashSet<FlexibleRequestMethod> _GET_POST = new HashSet<FlexibleRequestMethod> { new FlexibleRequestMethod(RequestMethod.GET), new FlexibleRequestMethod(RequestMethod.POST) };

        #region Get-/Setters

        public ResponseModification? Modification { get; }

        public abstract string? Title { get; }

        public abstract FlexibleContentType? ContentType { get; }

        protected abstract HashSet<FlexibleRequestMethod>? SupportedMethods { get; }

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
            if (SupportedMethods != null)
            {
                if (request.Method.KnownMethod != RequestMethod.HEAD)
                {
                    if (!SupportedMethods.Contains(request.Method))
                    {
                        return request.Respond(ResponseStatus.MethodNotAllowed);
                    }
                }
            }

            var response = HandleInternal(request);

            Modification?.Invoke(response);

            return response;
        }

        protected abstract IResponseBuilder HandleInternal(IRequest request);

        #endregion

    }

}
