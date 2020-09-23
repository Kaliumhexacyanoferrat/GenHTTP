using System.Collections.Generic;

using GenHTTP.Api.Protocol;
using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol
{

    public class RequestProperties : IRequestProperties
    {
        private PooledDictionary<string, object>? _Data;

        #region Get-/Setters

        public object this[string key] 
        {
            get
            {
                return (Data.ContainsKey(key)) ? Data[key] : throw new KeyNotFoundException($"Key '{key}' does not exist in request properties");
            }
            set 
            {
                Data[key] = value; 
            }
        }

        private PooledDictionary<string, object> Data =>  _Data ?? (_Data = new PooledDictionary<string, object>());

        #endregion

        #region Functionality

        public bool TryGet<T>(string key, out T entry)
        {
            if (Data.ContainsKey(key))
            {
                if (Data[key] is T result) 
                {
                    entry = result;
                    return true;
                } 
            }

#pragma warning disable CS8653, CS8601
            entry = default;
#pragma warning restore

            return false;
        }

        #endregion

    }

}
