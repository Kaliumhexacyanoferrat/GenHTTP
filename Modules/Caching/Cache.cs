using System.IO;

using GenHTTP.Modules.Caching.Memory;

namespace GenHTTP.Modules.Caching
{

    public static class Cache
    {

        public static IMemoryCacheBuilder<T> Memory<T>()
        {
            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return new StreamMemoryCacheBuilder<T>();
            }
            else
            {
                return new MemoryCacheBuilder<T>();
            }
        }

    }

}
