using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

using GenHTTP.Api.Infrastructure;
using GenHTTP.Api.Routing;

namespace GenHTTP.Api.Protocol
{

    public interface IRequest : IDisposable
    {

        #region General Infrastructure

        IServer Server { get; }

        IEndPoint EndPoint { get; }

        IClient Client { get; }

        IRoutingContext? Routing { get; set; }

        #endregion

        #region HTTP Protocol

        ProtocolType ProtocolType { get; }

        FlexibleRequestMethod Method { get; }

        string Path { get; }

        #endregion

        #region Headers

        string? UserAgent { get; }
        
        string? Referer { get; }

        string? Host { get; }

        string? this[string additionalHeader] { get; }

        IReadOnlyDictionary<string, string> Query { get; }

        ICookieCollection Cookies { get; }

        IHeaderCollection Headers { get; }

        #endregion

        #region Body

        Stream? Content { get; }

        #endregion

        #region Functionality

        IResponseBuilder Respond();

        #endregion

    }

}
