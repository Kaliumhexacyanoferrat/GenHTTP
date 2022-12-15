using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using GenHTTP.Api.Protocol;

using GenHTTP.Engine.Utilities;

namespace GenHTTP.Engine.Protocol
{

    public sealed class RequestProperties : IRequestProperties
    {
        private PooledDictionary<string, object>? _Data;

        private bool _Disposed;

        #region Get-/Setters

        public object this[string key] 
        {
            get
            {
                if (Data.TryGetValue(key, out var value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException($"Key '{key}' does not exist in request properties");
                }
            }
            set 
            {
                Data[key] = value; 
            }
        }

        private PooledDictionary<string, object> Data =>  _Data ??= new();

        #endregion

        #region Functionality

        public bool TryGet<T>(string key, [MaybeNullWhen(returnValue: false)] out T entry)
        {
            if (Data.ContainsKey(key))
            {
                if (Data[key] is T result) 
                {
                    entry = result;
                    return true;
                } 
            }

            entry = default;
            return false;
        }

        #endregion

        #region Disposal

        private void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Data?.Dispose();
                }

                _Data = null;
                _Disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            System.GC.SuppressFinalize(this);
        }

        #endregion

    }

}
