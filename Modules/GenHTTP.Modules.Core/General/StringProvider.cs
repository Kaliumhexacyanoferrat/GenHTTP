using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class StringProvider : ContentProviderBase
    {

        #region Get-/Setters
        
        public ContentType ContentType { get; }

        private byte[] Buffer { get; }

        #endregion

        #region Initialization

        public StringProvider(string data, ContentType contentType, ResponseModification? mod) : base(mod)
        {
            Buffer = Encoding.UTF8.GetBytes(data);
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        protected override IResponseBuilder HandleInternal(IRequest request)
        {
            return request.Respond()
                          .Content(new MemoryStream(Buffer), ContentType);
        }

        #endregion

    }

}
