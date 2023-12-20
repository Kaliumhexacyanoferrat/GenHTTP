using GenHTTP.Modules.AutoLayout.Provider;
using GenHTTP.Modules.AutoLayout.Scanning;

namespace GenHTTP.Modules.AutoLayout
{

    public static class Resolvers
    {

        public static HandlerRegistryBuilder Default()
        {
            return new HandlerRegistryBuilder().Fallback(new DownloadProvider())
                                               .Add(new PlainProvider())
                                               .Add(new MarkdownProvider())
                                               .Add(new ScribanProvider());
        }

        public static HandlerRegistryBuilder Empty() => new();

    }

}
