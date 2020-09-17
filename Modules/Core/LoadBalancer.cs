using GenHTTP.Modules.Core.LoadBalancing;

namespace GenHTTP.Modules.Core
{
    
    public static class LoadBalancer
    {

        public static LoadBalancerBuilder Create() => new LoadBalancerBuilder();

    }

}
