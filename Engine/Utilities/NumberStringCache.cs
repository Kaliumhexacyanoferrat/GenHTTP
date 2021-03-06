﻿using System.Collections.Concurrent;

namespace GenHTTP.Engine.Utilities
{

    public static class NumberStringCache
    {
        private static readonly ConcurrentDictionary<int, string> _Cache = new();

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

