using System.Collections.Concurrent;

namespace GenHTTP.Engine.Utilities
{

    public static class NumberStringCache
    {
        private static ConcurrentDictionary<int, string> _Cache = new ConcurrentDictionary<int, string>();

        #region Functionality

        public static string Convert(int number)
        {
            if (_Cache.TryGetValue(number, out var str))
            {
                return str;
            }

            var value = number.ToString();

            _Cache.TryAdd(number, value);

            return value;
        }

        #endregion

    }

}

