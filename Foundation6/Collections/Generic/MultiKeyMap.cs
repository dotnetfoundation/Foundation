﻿using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Foundation.Collections.Generic
{
    /// <summary>
    /// Dictionary that allows duplicate keys.
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class MultiKeyMap<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
    {
        private record KeyTuple(TKey Key, int Number);

        private readonly IDictionary<KeyTuple, TValue> _values;
        private readonly IMultiValueMap<TKey, KeyTuple> _keys;

        public MultiKeyMap()
        {
            _keys = new MultiValueMap<TKey, KeyTuple>();
            _values = new Dictionary<KeyTuple, TValue>();
        }

        public MultiKeyMap(int capacity)
        {
            _keys = new MultiValueMap<TKey, KeyTuple>(capacity);
            _values = new Dictionary<KeyTuple, TValue>(capacity);
        }

        public MultiKeyMap(IEqualityComparer<TKey> comparer)
        {
            _keys = new MultiValueMap<TKey, KeyTuple>(comparer);
            _values = new Dictionary<KeyTuple, TValue>();
        }

        public MultiKeyMap(int capacity, IEqualityComparer<TKey> comparer)
        {
            _keys = new MultiValueMap<TKey, KeyTuple>(capacity, comparer, () => new List<KeyTuple>());
            _values = new Dictionary<KeyTuple, TValue>(capacity);
        }

        public MultiKeyMap(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            items.ThrowIfNull();

            _values = items.Enumerate().ToDictionary(x => new KeyTuple(x.item.Key, 0), x => x.item.Value);
            _keys = _values.Keys.ToMultiValueMap(x => x.Key, x => x);
        }

        public TValue this[TKey key]
        {
            get
            {
                var tuple = _keys[key];
                return _values[tuple];
            }
            set
            {
                if (!((IDictionary<TKey, KeyTuple>)_keys).TryGetValue(key, out var tuple))
                    tuple = new KeyTuple(key, 0);

                _values[tuple] = value;
            }
        }

        public ICollection<TKey> Keys => _keys.Keys;

        public ICollection<TValue> Values => _values.Values;

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            var max = 0 == _keys.Count ? 0 : _keys.GetValues(key).Select(x => x.Number).Max() + 1;

            var tuple = new KeyTuple(key, max);
            _keys.Add(key, tuple);
            _values.Add(tuple, value);
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _keys.Clear();
            _values.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            if (!_keys.TryGetValues(item.Key, out var tuples)) return false;

            var values = tuples.Select(x => _values[x]);
            return values.Any(x => x.EqualsNullable(item.Value));
        }

        public bool ContainsKey(TKey key) => _keys.ContainsKey(key);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            var it = GetEnumerator();
            for(var i = arrayIndex; i < array.Length; i++)
            {
                if (!it.MoveNext()) break;

                array[i] = it.Current;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach(var tuple in _keys.Values)
            {
                var value = _values[tuple];
                yield return new KeyValuePair<TKey, TValue>(tuple.Key, value);
            }
        }

        public bool Remove(TKey key)
        {
            if (!_keys.TryGetValues(key, out var tuples)) return false;

            foreach(var tuple in tuples)
            {
                _values.Remove(tuple);
            }

            return true;
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (!_keys.TryGetValues(item.Key, out var tuples)) return false;

            foreach(var tuple in tuples)
            {
                var value = _values[tuple];
                if(item.Value.EqualsNullable(value))
                {
                    return _values.Remove(Pair.New(tuple, value));
                }
            }

            return false;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            if (!((IDictionary<TKey, KeyTuple>)_keys).TryGetValue(key, out var tuple))
            {
                value = default;
                return false;
            }

            value = _values[tuple];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
