using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class StringProvider : ContentProviderBase
    {

        #region Get-/Setters
        
        private StringContent Content { get; }

        public override string? Title => null;

        public override FlexibleContentType? ContentType { get; }

        #endregion

        #region Initialization

        public StringProvider(string data, FlexibleContentType contentType, ResponseModification? mod) : base(mod)
        {
            Content = new StringContent(data);
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            return request.Respond()
                          .Content(Content)
                          .Type(ContentType!.Value);
        }

        #endregion

    }

}
