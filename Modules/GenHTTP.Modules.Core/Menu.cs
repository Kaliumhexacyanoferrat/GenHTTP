using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Core.Websites;

namespace GenHTTP.Modules.Core
{

    public static class Menu
    {
        
        public static GeneratedMenuBuilder From(IHandler handler) => new GeneratedMenuBuilder().Handler(handler);

        public static StaticMenuBuilder Empty() => new StaticMenuBuilder();

    }

}
