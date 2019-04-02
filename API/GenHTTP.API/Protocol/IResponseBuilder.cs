using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Protocol
{

    public interface IResponseBuilder : IBuilder<IResponse>
    {

        IRequest Request { get; }

        IResponseBuilder Type(ResponseStatus type);

        IResponseBuilder Header(string key, string value);

        IResponseBuilder Expires(DateTime expiryDate);

        IResponseBuilder Modified(DateTime modificationDate);
        
        IResponseBuilder Cookie(Cookie cookie);
        
        IResponseBuilder Content(Stream body, ContentType contentType);

        IResponseBuilder Content(Stream body, ulong length, ContentType contentType);

        IResponseBuilder Encoding(string encoding);

    }

}
