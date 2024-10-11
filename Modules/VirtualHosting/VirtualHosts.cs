using GenHTTP.Modules.VirtualHosting.Provider;

namespace GenHTTP.Modules.VirtualHosting;

public static class VirtualHosts
{

    public static VirtualHostRouterBuilder Create()
    {
            return new VirtualHostRouterBuilder();
        }

}
