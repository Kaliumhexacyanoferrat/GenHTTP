using System;

namespace GenHTTP.Engine.Protocol
{
    
    /// <summary>
    /// Caches the value of the date header for one second
    /// before creating a new value, saving some allocations.
    /// </summary>
    public static class DateHeader
    {
        private static string _Value = string.Empty;

        private static byte _Second = 61;

        #region Functionality

        public static string GetValue()
        {
            var now = DateTime.UtcNow;

            var second = now.Second;

            if (second != _Second)
            {
                _Second = (byte)second;
                _Value = now.ToString("r");
            }

            return _Value;
        }

        #endregion

    }

}
