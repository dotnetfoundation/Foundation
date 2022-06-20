﻿namespace Foundation.Collections.Generic;

public static class Pair
{
    public static IEnumerable<KeyValuePair<TKey, TValue>> CreateMany<TKey, TValue>(params (TKey, TValue)[] keyValues)
    {
        foreach(var (key, value) in keyValues)
            yield return new KeyValuePair<TKey, TValue>(key, value);
    }

    public static KeyValuePair<TKey, TValue> Empty<TKey, TValue>() where TKey : notnull
    {
        return new KeyValuePair<TKey, TValue>();
    }

    public static bool IsEmtpy<TKey, TValue>(this KeyValuePair<TKey, TValue> pair) where TKey : notnull
    {
        return pair.Equals(Empty<TKey, TValue>());
    }

    public static bool IsInitialized<TKey, TValue>(this KeyValuePair<TKey, TValue> pair) where TKey : notnull
    {
        var empty = Empty<TKey, TValue>();
        return !(pair.Key.EqualsNullable(empty.Key) && pair.Value.EqualsNullable(empty.Value));
    }

    public static KeyValuePair<TKey, TValue> New<TKey, TValue>(TKey key, TValue value) where TKey : notnull
    {
        return new KeyValuePair<TKey, TValue>(key, value);
    }

    public static KeyValuePair<TKey, TValue> ThrowIfNotInitialized<TKey, TValue>(
        this KeyValuePair<TKey, TValue> pair,
        string name)
        where TKey : notnull
    {
        name.ThrowIfNullOrEmpty(nameof(name));

        if (!pair.IsInitialized()) throw new ArgumentException("not initialized", name);
        return pair;
    }
}
