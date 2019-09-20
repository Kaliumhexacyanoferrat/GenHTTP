
using System;
using System.Collections.Generic;
using System.Text;

using GenHTTP.Api.Routing;
using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Routing
{
    
    /// <summary>
    /// A builder which will provide an <see cref="IRouter"/>.
    /// </summary>
    public interface IRouterBuilder : IRouterBuilder<IRouter>
    {

    }

    public interface IRouterBuilder<T> : IBuilder<T> where T : IRouter
    {

    }

}
