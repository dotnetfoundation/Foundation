﻿namespace Foundation.Collections;

using Foundation.Collections.Generic;
using System.Collections;
using System.Diagnostics.CodeAnalysis;

public static class EnumerableExtensions
{
    public static IEnumerable AfterEveryObject(this IEnumerable items, [DisallowNull] Action action)
    {
        action.ThrowIfNull(nameof(action));

        var it = items.GetEnumerator();
        var next = it.MoveNext();
        while (next)
        {
            yield return it.Current;
            next = it.MoveNext();
            if (next)
                action();
        }
    }

    public static bool AnyObject(this IEnumerable items)
    {
        foreach (var _ in items)
            return true;

        return false;
    }

    public static bool AnyObject(this IEnumerable items, [DisallowNull] Func<object, bool> predicate)
    {
        predicate.ThrowIfNull(nameof(predicate));

        foreach (var item in items)
        {
            if (predicate(item))
                return true;
        }
        return false;
    }

    public static IEnumerable<T> CastTo<T>(this IEnumerable items)
    {
        if(null == items) return Enumerable.Empty<T>();

        return items.SelectObject(obj => (T)obj);
    }

    public static IEnumerable<T> FilterType<T>(this IEnumerable items)
    {
        if (null == items) yield break;
        
        foreach(var item in items)
        {
            if (item is T t) yield return t;
        }
    }

    public static object FirstObject(this IEnumerable items)
    {
        var item = FirstOrDefaultObject(items);
        if (item.IsNone) throw new InvalidOperationException("sequence is emtpy");

        return item.Value!;
    }

    private static Opt<object> FirstOrDefaultObject(this IEnumerable items)
    {
        if (null == items) throw new ArgumentNullException(nameof(items));

        var enumerator = items.GetEnumerator();
        if (enumerator.MoveNext())
        {
            return Opt.Some(enumerator.Current);
        }

        return Opt.None<object>();
    }

    public static void ForEachObject(this IEnumerable items, [DisallowNull] Action<object> action)
    {
        action.ThrowIfNull(nameof(action));

        foreach (var item in items)
            action(item);
    }

    /// <summary>
    /// Ignores item if predicate returns true.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="predicate"></param>
    /// <returns></returns>
    public static IEnumerable<T> Ignore<T>(this IEnumerable<T> items, [DisallowNull] Func<T, bool> predicate)
    {
        predicate.ThrowIfNull(nameof(predicate));

        foreach (var item in items)
        {
            if (predicate(item))
                continue;

            yield return item;
        }
    }

    /// <summary>
    /// Ignores items at indicies.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="indices"></param>
    /// <returns></returns>
    public static IEnumerable<T> Ignore<T>(this IEnumerable<T> items, params int[] indices)
    {
        if (0 == indices.Length) yield break;

        var i = 0;
        foreach (var item in items)
        {
            if (indices.Contains(i))
                continue;

            yield return item;
            i++;
        }
    }

    public static bool IsNullOrEmpty(this IEnumerable items)
    {
        if (items == null) return true;
        return !items.GetEnumerator().MoveNext();
    }

    public static IEnumerable<T> OfType<T>(this IEnumerable items)
    {
        foreach (var item in items)
        {
            if (item is T t) yield return t;
        }
    }

    public static IEnumerable<object> OfTypes(this IEnumerable items, params Type[] types)
    {
        Type? type = default;
        foreach (var item in items.OnFirstObject(i => type = i.GetType()))
        {
            if (types.Any(t => t.Equals(type) || t.IsAssignableFrom(type))) continue;
            yield return item;
        }
    }

    public static IEnumerable<T> OfTypes<T>(this IEnumerable items, params Type[] types)
    {
        foreach (var item in items.OfTypes(types))
        {
            if (item is T t) yield return t;
        }
    }

    /// <summary>
    /// Calls action on first item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable OnFirstObject(this IEnumerable items, [DisallowNull] Action action)
    {
        action.ThrowIfNull(nameof(action));

        var it = items.GetEnumerator();
        if (!it.MoveNext()) yield break;

        action();
        yield return it.Current;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// Calls action on first item.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="action"></param>
    /// <returns></returns>
    public static IEnumerable OnFirstObject(this IEnumerable items, [DisallowNull] Action<object> action)
    {
        action.ThrowIfNull(nameof(action));

        var it = items.GetEnumerator();
        if (!it.MoveNext()) yield break;

        action(it.Current);
        yield return it.Current;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    public static IEnumerable SelectNotNull(this IEnumerable items)
    {
        foreach (object item in items)
        {
            if (null != item) yield return item;
        }
    }

    public static IEnumerable SelectObject(this IEnumerable items, [DisallowNull] Func<object, object> selector)
    {
        selector.ThrowIfNull(nameof(selector));

        foreach (var item in items)
            yield return selector(item);
    }

    public static IEnumerable<T> SelectObject<T>(this IEnumerable items, [DisallowNull] Func<object, T> selector)
    {
        selector.ThrowIfNull(nameof(selector));

        foreach (var item in items)
            yield return selector(item);
    }

    public static IEnumerable SelectObjectByIndex(this IEnumerable items, [DisallowNull] Func<long, bool> selector)
    {
        selector.ThrowIfNull(nameof(selector));

        long i = 0;
        return items.WhereObject(item => selector(i++));
    }

    public static object SingleObject(this IEnumerable items)
    {
        if (null == items) throw new ArgumentNullException(nameof(items));

        var enumerator = items.GetEnumerator();
        if (enumerator.MoveNext())
        {
            var current = enumerator.Current;
            if(enumerator.MoveNext()) throw new InvalidOperationException("more than one element");

            return current;
        }

        throw new InvalidOperationException("no element");
    }

    public static IEnumerable<T> ToEnumerable<T>(this IEnumerable items) => items.CastTo<T>();

    public static IList<T> ToList<T>(this IEnumerable items)
    {
        var list = new List<T>();
        foreach (var item in items)
            list.Add((T)item);

        return list;
    }

    public static object?[] ToObjectArray(this IEnumerable items)
    {
        var list = new ArrayList();
        items.ForEachObject(i => list.Add(i));
        return list.ToArray();
    }

    public static IList ToObjectList(this IEnumerable items)
    {
        var list = new ArrayList();
        items.ForEachObject(i => list.Add(i));
        return list;
    }

    public static IReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> items)
    {
        return ReadOnlyCollection.New(items);
    }

    public static IEnumerable WhereObject(this IEnumerable items, [DisallowNull] Func<object, bool> selector)
    {
        selector.ThrowIfNull(nameof(selector));

        foreach (var item in items)
        {
            if (selector(item))
                yield return item;
        }
    }
}

