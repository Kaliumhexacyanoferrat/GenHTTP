using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Protocol
{

    public interface IRequest : IDisposable
    {

        ProtocolType ProtocolType { get; }

        RequestType Type { get; }

        string Path { get; }

        string? UserAgent { get; }
        
        string? Referer { get; }

        string? Host { get; }

        string? this[string additionalHeader] { get; }

        IReadOnlyDictionary<string, string> Query { get; }

        ICookieCollection Cookies { get; }

        IHeaderCollection Headers { get; }

        Stream? Content { get; }

        IClientHandler Handler { get; }

        IRoutingContext? Routing { get; set; }
        
        IResponseBuilder Respond();

    }

}
