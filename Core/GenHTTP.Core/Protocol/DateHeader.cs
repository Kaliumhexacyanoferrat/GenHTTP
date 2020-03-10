using System;
using System.Threading;

namespace GenHTTP.Core.Protocol
{
    
    internal static class DateHeader
    {

        private static readonly Timer _Timer = new Timer((s) => { UpdateValue(); }, null, 1000, 1000);

        #region Get-/Setters

        internal static string Value { get; private set; }

        #endregion

        #region Initialization

        static DateHeader()
        {
            Value = GetValue();
        }

        #endregion

        #region Functionality

        private static void UpdateValue()
        {
            Value = GetValue();
        }

        private static string GetValue()
        {
            return DateTime.UtcNow.ToString("r");
        }

        #endregion

    }

}
