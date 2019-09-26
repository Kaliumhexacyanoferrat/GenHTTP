using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class StringProvider : ContentProviderBase
    {

        #region Get-/Setters
        
        private byte[] Buffer { get; }

        public override string? Title => null;

        public override FlexibleContentType ContentType { get; }

        #endregion

        #region Initialization

        public StringProvider(string data, FlexibleContentType contentType, ResponseModification? mod) : base(mod)
        {
            Buffer = Encoding.UTF8.GetBytes(data);
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            return request.Respond()
                          .Content(new MemoryStream(Buffer), ContentType.RawType);
        }

        #endregion

    }

}
