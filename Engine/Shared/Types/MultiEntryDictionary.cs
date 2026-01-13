using System.Collections;

namespace GenHTTP.Engine.Shared.Types;

public class MultiEntryDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue> where TKey : IEquatable<TKey>
{
    private readonly List<KeyValuePair<TKey, TValue>> _entries;

    private readonly IEqualityComparer<TKey> _comparer;

    #region Get-/Setters

    private bool HasEntries => _entries.Count > 0;

    public virtual TValue this[TKey key]
    {
        get
        {
            if (HasEntries)
            {
                for (var i = 0; i < _entries.Count; i++)
                {
                    if (_comparer.Equals(_entries[i].Key, key))
                    {
                        return _entries[i].Value;
                    }
                }
            }

            throw new KeyNotFoundException();
        }
        set
        {
            if (HasEntries)
            {
                for (var i = 0; i < _entries.Count; i++)
                {
                    if (_comparer.Equals(_entries[i].Key, key))
                    {
                        _entries[i] = new KeyValuePair<TKey, TValue>(key, value);
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
            var result = new HashSet<TKey>(_entries.Count);

            if (HasEntries)
            {
                for (var i = 0; i < _entries.Count; i++)
                {
                    result.Add(_entries[i].Key);
                }
            }

            return result;
        }
    }

    public ICollection<TValue> Values
    {
        get
        {
            var result = new List<TValue>(_entries.Count);

            if (HasEntries)
            {
                for (var i = 0; i < _entries.Count; i++)
                {
                    result.Add(_entries[i].Value);
                }
            }

            return result;
        }
    }

    public int Count => _entries.Count;

    public bool IsReadOnly => false;

    IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;

    IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

    public int Capacity => _entries.Capacity;

    #endregion

    #region Initialization

    public MultiEntryDictionary(int initialCapacity, IEqualityComparer<TKey> comparer)
    {
        _entries = new(initialCapacity);
        _comparer = comparer;
    }

    #endregion

    #region Functionality

    public virtual void Add(TKey key, TValue value)
    {
        _entries.Add(new KeyValuePair<TKey, TValue>(key, value));
    }

    public virtual void Add(KeyValuePair<TKey, TValue> item)
    {
        _entries.Add(item);
    }

    public void Clear()
    {
        _entries.Clear();
    }

    public bool Contains(KeyValuePair<TKey, TValue> item)
    {
        if (HasEntries)
        {
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_comparer.Equals(_entries[i].Key, item.Key))
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
            for (var i = 0; i < _entries.Count; i++)
            {
                if (_comparer.Equals(_entries[i].Key, key))
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

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _entries.GetEnumerator();

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

    IEnumerator IEnumerable.GetEnumerator() => _entries.GetEnumerator();

    public bool ContainsMultiple(TKey key)
    {
        var found = false;

        for (var i = 0; i < _entries.Count; i++)
        {
            if (_comparer.Equals(_entries[i].Key, key))
            {
                if (found)
                {
                    return true;
                }

                found = true;
            }
        }

        return false;
    }

    #endregion

}
