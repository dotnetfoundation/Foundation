﻿namespace Foundation.Collections.Generic;

using System.Collections;
using System.Diagnostics.CodeAnalysis;

public static class ArrayValue
{
    public static ArrayValue<T> New<T>(params T[] values)
    {
        return new ArrayValue<T>(values);
    }
}

/// <summary>
/// This is an immutable array that compares each element on <see cref="Equals(ArrayValue{T})"/>.
/// That enables the comparison of two arrays.
/// </summary>
/// <typeparam name="T"></typeparam>
public struct ArrayValue<T>
    : ICloneable
    , IEnumerable<T>
    , IEquatable<ArrayValue<T>>
    , IEquatable<T[]>
{
    private readonly int _hashCode;
    private string _valuesAsString;
    private readonly T[] _values;

    public ArrayValue(T[] values)
    {
        _values = values.ThrowIfNull();
        _hashCode = HashCode.FromObjects(_values);
        _valuesAsString = "";
    }

    public static implicit operator ArrayValue<T>(T[] array) => ArrayValue.New(array);

    public static implicit operator T[](ArrayValue<T> array) => array._values;

    public static bool operator ==(ArrayValue<T> left, ArrayValue<T> right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ArrayValue<T> left, ArrayValue<T> right)
    {
        return !(left == right);
    }

    public T this[int index] => _values[index];

    public object Clone()
    {
        return IsEmpty
            ? new ArrayValue<T>(Array.Empty<T>())
            : new ArrayValue<T>((T[])_values.Clone());
    }

    public override bool Equals([NotNullWhen(true)] object? obj) => obj is ArrayValue<T> other && Equals(other);

    public bool Equals(T[]? other)
    {
        if (IsEmpty) return null == other || 0 == other.Length;

        return null != other && _values.SequenceEqual(other);
    }

    public bool Equals(ArrayValue<T> other)
    {
        if (IsEmpty) return other.IsEmpty;
        if (other.IsEmpty) return false;

        if (GetHashCode() != other.GetHashCode()) return false;

        return _values.SequenceEqual(other._values);
    }

    public IEnumerator<T> GetEnumerator() => _values.GetEnumerator<T>();

    IEnumerator IEnumerable.GetEnumerator() => _values.GetEnumerator();

    public override int GetHashCode() => _hashCode;

    public bool IsEmpty => null == _values || 0 == _values.Length;

    public int Length => IsEmpty ? 0 : _values.Length;

    public override string? ToString()
    {
        if (IsEmpty || 0 == _values.Length) return "";
        
        if(0 == _valuesAsString.Length)
            _valuesAsString = string.Join(", ", _values);

        return _valuesAsString;
    }
}