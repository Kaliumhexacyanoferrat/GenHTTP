using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;

namespace GenHTTP.Core.Utilities
{

    internal class PooledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>>, IDisposable where TKey : IEquatable<TKey>
    {
        private static readonly ArrayPool<KeyValuePair<TKey, TValue>> POOL = ArrayPool<KeyValuePair<TKey, TValue>>.Shared;

        private short _Enumerator = -1;
        private ushort _Index = 0;

        private KeyValuePair<TKey, TValue>[] _Entries;

        private readonly IEqualityComparer<TKey> _Comparer;

        #region Get-/Setters

        public TValue this[TKey key]
        {
            get
            {
                for (int i = 0; i < _Index; i++)
                {
                    if (_Comparer.Equals(_Entries[i].Key, key))
                    {
                        return _Entries[i].Value;
                    }
                }

                throw new KeyNotFoundException();
            }
            set
            {
                for (int i = 0; i < _Index; i++)
                {
                    if (_Comparer.Equals(_Entries[i].Key, key))
                    {
                        _Entries[i] = new KeyValuePair<TKey, TValue>(key, value);
                        return;
                    }
                }

                Add(key, value);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                var result = new List<TKey>(_Index);

                for (int i = 0; i < _Index; i++)
                {
                    result.Add(_Entries[i].Key);
                }

                return result;
            }
        }

        public ICollection<TValue> Values
        {
            get
            {
                var result = new List<TValue>(_Index);

                for (int i = 0; i < _Index; i++)
                {
                    result.Add(_Entries[i].Value);
                }

                return result;
            }
        }

        public int Count => _Index;

        public bool IsReadOnly => false;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public KeyValuePair<TKey, TValue> Current => _Entries[_Enumerator];

        object IEnumerator.Current => _Entries[_Enumerator];

        public int Capacity { get; private set; }

        #endregion

        #region Initialization

        internal PooledDictionary(int initialCapacity, IEqualityComparer<TKey> comparer)
        {
            Capacity = initialCapacity;

            _Comparer = comparer;
            _Entries = POOL.Rent(initialCapacity);
        }

        #endregion

        #region Functionality

        public void Add(TKey key, TValue value)
        {
            CheckResize();
            _Entries[_Index++] = new KeyValuePair<TKey, TValue>(key, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            CheckResize();
            _Entries[_Index++] = item;
        }

        public void Clear()
        {
            _Index = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            for (int i = 0; i < _Index; i++)
            {
                if (_Comparer.Equals(_Entries[i].Key, item.Key))
                {
                    return true;
                }
            }

            return false;
        }

        public bool ContainsKey(TKey key)
        {
            for (int i = 0; i < _Index; i++)
            {
                if (_Comparer.Equals(_Entries[i].Key, key))
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            _Enumerator = 0;
            return this;
        }

        public bool Remove(TKey key)
        {
            throw new NotSupportedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotSupportedException();
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if (ContainsKey(key))
            {
                value = this[key];
                return true;
            }

#pragma warning disable CS8653
            value = default;
#pragma warning restore

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this;
        }

        public bool MoveNext()
        {
            _Enumerator++;
            return (_Enumerator < _Index);
        }

        public void Reset()
        {
            _Enumerator = -1;
        }

        private void CheckResize()
        {
            if (_Index >= _Entries.Length)
            {
                var oldEntries = _Entries;

                try
                {
                    Capacity *= 2;

                    _Entries = POOL.Rent(Capacity);

                    for (int i = 0; i < _Index; i++)
                    {
                        _Entries[i] = oldEntries[i];
                    }
                }
                finally
                {
                    POOL.Return(oldEntries);
                }
            }
        }

        #endregion

        #region IDisposable Support

        private bool disposedValue = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    POOL.Return(_Entries);
                }

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

        #endregion

    }

}
