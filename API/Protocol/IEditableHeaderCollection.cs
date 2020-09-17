using System;
using System.Collections.Generic;

namespace GenHTTP.Api.Protocol
{

    public interface IEditableHeaderCollection : IDictionary<string, string>, IDisposable
    {

    }

}
