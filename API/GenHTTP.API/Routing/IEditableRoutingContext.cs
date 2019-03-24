using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Content;

namespace GenHTTP.Api.Routing
{

    public interface IEditableRoutingContext : IRoutingContext
    {

        bool IsIndex { get; }

        void Scope(IRouter router);

        string? ScopeSegment(IRouter router);

        void Rewrite(string relativeUrl);

        void RegisterContent(IContentProvider contentProvider);

    }

}
