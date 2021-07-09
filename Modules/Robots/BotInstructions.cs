using GenHTTP.Modules.Robots.Provider;

namespace GenHTTP.Modules.Robots
{

    public static class BotInstructions
    {

        public const string FILE_NAME = "robots.txt";

        public static RobotsProviderBuilder Default() => new RobotsProviderBuilder().Allow();

        public static RobotsProviderBuilder Empty() => new();

    }

}
