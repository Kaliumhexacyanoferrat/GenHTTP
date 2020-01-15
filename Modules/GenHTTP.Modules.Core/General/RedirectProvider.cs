using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class RedirectProvider : ContentProviderBase
    {

        #region Get-/Setters

        public string Location { get; }

        public bool Temporary { get; }

        public override string? Title => null;

        public override FlexibleContentType? ContentType => null;

        #endregion

        #region Initialization

        public RedirectProvider(string location, bool temporary, ResponseModification? modification) : base(modification)
        {
            Location = location;
            Temporary = temporary;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            var response = request.Respond()
                                  .Header("Location", Location);

            if (Temporary)
            {
                return response.Status(ResponseStatus.TemporaryRedirect);
            }
            else
            {
                if (request.Method.KnownMethod == RequestMethod.GET)
                {
                    return response.Status(ResponseStatus.MovedPermanently);
                }
                else
                {
                    return response.Status(ResponseStatus.PermanentRedirect);
                }
            }
        }

        #endregion

    }

}
