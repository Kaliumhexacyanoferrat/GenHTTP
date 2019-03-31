using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Modules;
using GenHTTP.Api.Protocol;

namespace GenHTTP.Modules.Core.General
{

    public class StringProvider : IContentProvider
    {

        #region Get-/Setters

        public string Data { get; }

        public ContentType ContentType { get; }

        #endregion

        #region Initialization

        public StringProvider(string data, ContentType contentType)
        {
            Data = data;
            ContentType = contentType;
        }

        #endregion

        #region Functionality

        public IResponseBuilder Handle(IRequest request)
        {
            return request.Respond()
                          .Content(new MemoryStream(Encoding.UTF8.GetBytes(Data)), ContentType);
        }

        #endregion

    }

}
