using System.Buffers;
using System.Collections;

namespace GenHTTP.Engine.Shared.Types;

public class PooledDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>> where TKey : IEquatable<TKey>
{
    private static readonly ArrayPool<KeyValuePair<TKey, TValue>> Pool = ArrayPool<KeyValuePair<TKey, TValue>>.Shared;

    private short _enumerator = -1;
    private ushort _index;

    private KeyValuePair<TKey, TValue>[]? _entries;

    private readonly IEqualityComparer<TKey> _comparer;

    #region Get-/Setters

    private KeyValuePair<TKey, TValue>[] Entries => _entries ??= Pool.Rent(Capacity);

    private bool HasEntries => _entries is not null;

    public virtual TValue this[TKey key]
    {
        get
        {
            if (HasEntries)
            {
                for (var i = 0; i < _index; i++)
                {
                    if (_comparer.Equals(Entries[i].Key, key))
                    {
                        return Entries[i].Value;
                    }
                }
            }

            throw new KeyNotFoundException();
        }
        set
        {
            if (HasEntries)
            {
                for (var i = 0; i < _index; i++)
                {
                    if (_comparer.Equals(Entries[i].Key, key))
                    {
                        Entries[i] = new KeyValuePair<TKey, TValue>(key, value);
                        return;
                    }
                }
            }

            Add(key, value);
        }
    }

    public ICollection<TKey> Keys
    {
        get
        {
            var result = new List<TKey>(_index);

            if (HasEntries)
            {
                for (var i = 0; i < _index; i++)
                {
                    result.Add(Entries[i].Key);
                }
            }

            return result;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var result = new List<TValue>(_index);

            if (HasEntries)
            {
                for (var i = 0; i < _index; i++)
                {
                    result.Add(Entries[i].Value);
                }
            }

            return result;
        }
    }

    public int Count => _index;

    public bool IsReadOnly => false;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public KeyValuePair<TKey, TValue> Current => Entries[_enumerator];

    object IEnumerator.Current => Entries[_enumerator];

    public int Capacity { get; private set; }

    #endregion

    #region Initialization

    public PooledDictionary() : this(4, EqualityComparer<TKey>.Default)
    {

    }

    public PooledDictionary(int initialCapacity, IEqualityComparer<TKey> comparer)
    {
        Capacity = initialCapacity;

        _comparer = comparer;
    }

    #endregion

    #region Functionality

    public virtual void Add(TKey key, TValue value)
    {
        CheckResize();
        Entries[_index++] = new KeyValuePair<TKey, TValue>(key, value);
    }

    public virtual void Add(KeyValuePair<TKey, TValue> item)
    {
        CheckResize();
        Entries[_index++] = item;
    }

    public void Clear()
    {
        _index = 0;
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (HasEntries)
        {
            for (var i = 0; i < _index; i++)
            {
                if (_comparer.Equals(Entries[i].Key, item.Key))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public bool ContainsKey(TKey key)
    {
        if (HasEntries)
        {
            for (var i = 0; i < _index; i++)
            {
                if (_comparer.Equals(Entries[i].Key, key))
                {
                    return true;
                }
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
        _enumerator = -1;
        return this;
    }

    public bool Remove(TKey key) => throw new NotSupportedException();

    public bool Remove(KeyValuePair<TKey, TValue> item) => throw new NotSupportedException();

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (ContainsKey(key))
        {
            value = this[key];
            return true;
        }

#pragma warning disable CS8653, CS8601
        value = default;
#pragma warning restore

        return false;
    }

    IEnumerator IEnumerable.GetEnumerator() => this;

    public bool MoveNext()
    {
        _enumerator++;
        return _enumerator < _index;
    }

    public void Reset()
    {
        _enumerator = -1;
    }

    private void CheckResize()
    {
        if (_index >= Entries.Length)
        {
            var oldEntries = Entries;

            try
            {
                if (oldEntries.Length > Capacity)
                {
                    Capacity = oldEntries.Length * 2;
                }
                else
                {
                    Capacity *= 2;
                }

                _entries = Pool.Rent(Capacity);

                for (var i = 0; i < _index; i++)
                {
                    Entries[i] = oldEntries[i];
                }
            }
            finally
            {
                Pool.Return(oldEntries);
            }
        }
    }

    #endregion

    #region IDisposable Support

    private bool _disposed;

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                if (HasEntries)
                {
                    Pool.Return(Entries);
                }
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    #endregion

}
