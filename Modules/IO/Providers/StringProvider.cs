using System.Collections.Generic;

using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;

using GenHTTP.Modules.Basics;

namespace GenHTTP.Modules.IO.Providers
{

    public class StringProvider : IHandler
    {

        #region Get-/Setters

        public IHandler Parent { get; }

        private StringContent Content { get; }

        private FlexibleContentType ContentType { get; }

        #endregion

        #region Initialization

        public StringProvider(IHandler parent, string data, FlexibleContentType contentType)
        {
            Parent = parent;

            Content = new StringContent(data);
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        public IResponse? Handle(IRequest request)
        {
            return request.Respond()
                          .Content(Content)
                          .Type(ContentType)
                          .Build();
        }

        public IEnumerable<ContentElement> GetContent(IRequest request) => this.GetContent(request, "String Data", null, Api.Protocol.ContentType.TextPlain);

        #endregion

    }

}
