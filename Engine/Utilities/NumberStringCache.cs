using System.Collections.Concurrent;

namespace GenHTTP.Engine.Utilities
{

    public static class NumberStringCache
    {
        private static readonly ConcurrentDictionary<ulong, string> _Cache = new();

        #region Functionality

        public static string Convert(int number) => Convert((ulong)number);

        public static string Convert(ulong number)
        {
            if (number <= 1024)
            {
                if (_Cache.TryGetValue(number, out var str))
                {
                    return str;
                }

                var value = number.ToString();

                _Cache.TryAdd(number, value);

                return value;
            }

            return $"{number}";
        }

        #endregion

    }

}

