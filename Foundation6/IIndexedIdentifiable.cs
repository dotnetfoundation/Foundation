﻿namespace Foundation;

/// <summary>
/// Contract for an identifiable object with a dynamic identifier definition.
/// Can be used for generic objects with different identifiers.
/// </summary>
/// <typeparam name="TKey">The key for the identifier. E.g. a name as string.</typeparam>
/// <typeparam name="TValue">The value of the identifier.</typeparam>
public interface IIndexedIdentifiable<TKey, TValue>
    where TKey : notnull
{
    KeyValue<TKey, TValue> Identifier { get; }
}

