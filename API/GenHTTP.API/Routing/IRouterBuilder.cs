using GenHTTP.Api.Infrastructure;

namespace GenHTTP.Api.Routing
{

    /// <summary>
    /// A builder which will provide an <see cref="IRouter"/>.
    /// </summary>
    public interface IRouterBuilder : IRouterBuilder<IRouter>
    {

    }

    public interface IRouterBuilder<out T> : IBuilder<T> where T : IRouter
    {

    }

}
