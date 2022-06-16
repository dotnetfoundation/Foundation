﻿namespace Foundation.Collections.Generic
{
    public static  class DictionaryExtensions
    {
        public static bool IsEqualTo<TKey, TValue>(
            this Dictionary<TKey, TValue> lhs, 
            IEnumerable<KeyValuePair<TKey, TValue>> rhs)
            where TKey : notnull
        {
            return IsEqualTo((IDictionary<TKey, TValue>)lhs, rhs);
        }

        /// <summary>
        /// Returns true if all keys with their values of lhs and rhs are equal.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool IsEqualTo<TKey, TValue>(this Dictionary<TKey, TValue> lhs, Dictionary<TKey, TValue> rhs)
            where TKey : notnull
        {
            return IsEqualTo((IDictionary<TKey, TValue>)lhs, (IDictionary<TKey, TValue>)rhs);
        }

        /// <summary>
        /// Returns true if all keys with their values of lhs and rhs are equal.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool IsEqualTo<TKey, TValue>(
            this IDictionary<TKey, TValue> lhs,
            IEnumerable<KeyValuePair<TKey, TValue>> rhs)
            where TKey : notnull
        {
            var rhsCount = 0;
            foreach (var r in rhs)
            {
                if (!lhs.TryGetValue(r.Key, out TValue? lhsValue)) return false;
                if (!lhsValue.EqualsNullable(r.Value)) return false;

                rhsCount++;

                if (lhs.Count < rhsCount) return false;
            }

            return lhs.Count == rhsCount;
        }

        /// <summary>
        /// Returns true if all keys with their values of lhs and rhs are equal.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="lhs"></param>
        /// <param name="rhs"></param>
        /// <returns></returns>
        public static bool IsEqualTo<TKey, TValue>(this IDictionary<TKey, TValue> lhs, IDictionary<TKey, TValue> rhs)
            where TKey : notnull
        {
            if (null == lhs) return null == rhs;
            if (null == rhs) return false;
            if (lhs.Count != rhs.Count) return false;

            foreach (var kvp in lhs)
            {
                if (!rhs.TryGetValue(kvp.Key, out TValue? rhsValue)) return false;
                if (!kvp.Value.EqualsNullable(rhsValue)) return false;
            }
            return true;
        }

        public static Opt<KeyValuePair<TKey, TValue>> RemovKey<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
            where TKey : notnull
        {
            dictionary.ThrowIfNull();

            if (!dictionary.TryGetValue(key, out TValue? value)) return Opt.None<KeyValuePair<TKey, TValue>>();
            
            dictionary.Remove(key);
            return Opt.Some(Pair.New(key, value));
        }

        public static IEnumerable<KeyValue<TKey, TValue>> ToKeyValues<TKey, TValue>(this IDictionary<TKey, TValue> dictionary)
            where TKey : notnull
        {
            return dictionary.Select(kvp => new KeyValue<TKey, TValue>(kvp.Key, kvp.Value));
        }
    }
}
