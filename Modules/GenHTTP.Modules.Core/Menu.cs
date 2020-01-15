using GenHTTP.Api.Routing;
using GenHTTP.Modules.Core.Websites;

namespace GenHTTP.Modules.Core
{

    public static class Menu
    {
        
        public static GeneratedMenuBuilder From(IRouter router) => new GeneratedMenuBuilder().Router(router);

        public static StaticMenuBuilder Empty() => new StaticMenuBuilder();

    }

}
