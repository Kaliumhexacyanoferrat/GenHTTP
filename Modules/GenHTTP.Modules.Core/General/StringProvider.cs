using GenHTTP.Api.Content;
using GenHTTP.Api.Protocol;
using System.Collections.Generic;

namespace GenHTTP.Modules.Core.General
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

        public IEnumerable<ContentElement> GetContent(IRequest request)
        {
            throw new System.NotImplementedException();
        }

        #endregion

    }

}
