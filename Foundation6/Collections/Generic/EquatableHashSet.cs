﻿namespace Foundation.Collections.Generic;

using Foundation;
using Foundation.ComponentModel;
using System.Runtime.Serialization;

/// <summary>
/// This hashset considers the equality of all elements <see cref="Equals"/>. The position of the elements are ignored.
/// </summary>
/// <typeparam name="T"></typeparam>
[Serializable]
public class EquatableHashSet<T>
    : HashSet<T>
    , ICollectionChanged<T>
    , IEquatable<EquatableHashSet<T>>
    , IEquatable<IEnumerable<T>>
    , ISerializable
{
    private int _hashCode;

    public EquatableHashSet() : base()
    {
        _hashCode = 0;
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection">EnumerableCounter is needed to detect wether the collection contains duplicates.
    /// Use T[], Collection<typeparamref name="T"/> or List<typeparamref name="T"/></param>
    public EquatableHashSet(IEnumerable<T> collection) : base(collection)
    {        
        _hashCode = CreateHashCode();
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    public EquatableHashSet(IEqualityComparer<T>? comparer) : base(comparer)
    {
        _hashCode = CreateHashCode();
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    public EquatableHashSet(int capacity) : base(capacity)
    {
        _hashCode = CreateHashCode();
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="collection">EnumerableCounter is needed to detect wether the collection contains duplicates.
    /// Use T[], Collection<typeparamref name="T"/> or List<typeparamref name="T"/></param>
    /// <param name="comparer"></param>
    public EquatableHashSet(IEnumerable<T> collection, IEqualityComparer<T>? comparer) : base(collection, comparer)
    {
        _hashCode = CreateHashCode();
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    public EquatableHashSet(int capacity, IEqualityComparer<T>? comparer)
        : base(capacity, comparer)
    {
        _hashCode = CreateHashCode();
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    public EquatableHashSet(SerializationInfo info, StreamingContext context) : base(info, context)
    {
        _hashCode = CreateHashCode();
        CollectionChanged = new Event<Action<CollectionEvent<T>>>();
    }

    /// <summary>
    /// Adds an item to the hashset.
    /// </summary>
    /// <param name="item"></param>
    public new bool Add(T item)
    {
        var added = base.Add(item.ThrowIfNull());
        if (added)
        {
            _hashCode = CreateHashCode();

            CollectionChanged?.Publish(new { State = CollectionActionState.Added, Element = item });
            return true;
        }

        return false;
    }

    public new void Clear()
    {
        base.Clear();

        _hashCode = CreateHashCode();

        CollectionChanged?.Publish(new { State = CollectionActionState.Cleared });
    }

    public Event<Action<CollectionEvent<T>>> CollectionChanged { get; private set; }

    protected int CreateHashCode()
    {
        return  HashCode.CreateBuilder()
                        .AddHashCode(DefaultHashCode)
                        .AddOrderedObjects(this)
                        .GetHashCode();
    }

    protected static int DefaultHashCode { get; } = typeof(EquatableHashSet<T>).GetHashCode();

    public override bool Equals(object? obj) => Equals(obj as EquatableHashSet<T>);

    /// <summary>
    /// Checks the equality of all elements. The position of the elements are ignored.
    /// </summary>
    /// <param name="other"></param>
    /// <returns></returns>
    public bool Equals(EquatableHashSet<T>? other)
    {
        if (null == other) return false;
        if (_hashCode != other._hashCode) return false;

        return SetEquals(other);
    }

    public bool Equals(IEnumerable<T>? other)
    {
        if (null == other) return false;

        return SetEquals(other);
    }

    /// <summary>
    /// Considers values only. Position of the values are ignored.
    /// </summary>
    /// <returns></returns>
    public override int GetHashCode() => _hashCode;

    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
    }

    public new bool Remove(T item)
    {
        item.ThrowIfNull();

        if (base.Remove(item))
        {
            _hashCode = CreateHashCode();
            CollectionChanged?.Publish(new { State = CollectionActionState.Removed, Element = item });
            return true;
        }
        return false;
    }
}
