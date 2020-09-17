using GenHTTP.Modules.Core.Bots;

namespace GenHTTP.Modules.Core
{

    public static class Robots
    {

        public static RobotsProviderBuilder Default() => new RobotsProviderBuilder().Allow();

        public static RobotsProviderBuilder Empty() => new RobotsProviderBuilder();

    }

}
