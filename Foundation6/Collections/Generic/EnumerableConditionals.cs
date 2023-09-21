﻿using System.Runtime.CompilerServices;

namespace Foundation.Collections.Generic;

public static class EnumerableConditionals
{
    private class ElseIf<T> : IElseIf<T>
    {
        private readonly IEnumerable<T> _items;

        public ElseIf(IEnumerable<T> items)
        {
            _items = items.ThrowIfEnumerableIsNull();
        }

        public IEnumerable<T> Else() => _items;

        public IEnumerable<T> Else(Action<T> action)
        {
            foreach (var item in _items)
            {
                action(item);
                yield return item;
            }
        }

        IElseIf<T> IElseIf<T>.ElseIf(Func<T, bool> condition, Action<T> action)
        {
            return _items.If(condition, action);
        }

        public void EndIf()
        {
            foreach (var _ in Else())
            {
            }
        }
    }

    private class ElseIf<T, TResult> : IElseIf<T, TResult>
    {
        private readonly IEnumerable<(T? lhs, TResult? rhs, bool isLhs)> _items;
        private readonly Func<T, TResult> _selector;

        public ElseIf(IEnumerable<(T? lhs, TResult? rhs, bool isLhs)> items, Func<T, TResult> selector)
        {
            _items = items.ThrowIfEnumerableIsNull();
            _selector = selector.ThrowIfNull();
        }

        public IEnumerable<TResult> Else() => _items.Where(x => x.isLhs).Select(tuple => _selector(tuple.lhs!));

        public IEnumerable<TResult> Else(Func<T, TResult> selector)
        {
            foreach (var (lhs, rhs, isLhs) in _items)
            {
                if (isLhs)
                {
                    if (lhs is null) continue;

                    var selected = selector(lhs);
                    yield return selected;
                    continue;
                }

                if(rhs is null) continue;

                yield return rhs;
            }
        }

        public void EndIf()
        {
            foreach (var _ in Else())
            {
            }
        }

        IElseIf<T, TResult> IElseIf<T, TResult>.ElseIf(Func<T, bool> condition, Func<T, TResult> selector)
        {
            return _items.Where(x => x.isLhs && x.lhs is not null)
                         .Select(x => x.lhs!)
                         .If(condition, selector);
        }
    }


    private class ElseResult<T, TResult> : IElse<T, TResult>
    {
        private readonly IEnumerable<T> _items;
        private readonly Func<T, bool> _predicate;
        private readonly Func<T, TResult> _mapIf;

        public ElseResult(
            IEnumerable<T> items,
            Func<T, bool> predicate,
            Func<T, TResult> mapIf)
        {
            _items = items.ThrowIfEnumerableIsNull();
            _predicate = predicate.ThrowIfNull();
            _mapIf = mapIf.ThrowIfNull();
        }

        public IEnumerable<TResult> Else(Func<T, TResult> map)
        {
            foreach (var item in _items)
            {
                yield return _predicate(item) ? _mapIf(item) : map(item);
            }
        }
    }

    /// <summary>
    /// Adds an item if the list is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<T> AddIfEmpty<T>(this IEnumerable<T> items, Func<T> factory)
    {
        factory.ThrowIfNull();

        var it = items.GetEnumerator();
        if (!it.MoveNext())
        {
            yield return factory();
        }
        else
        {
            yield return it.Current;

            while (it.MoveNext()) yield return it.Current;
        }
    }

    /// <summary>
    /// Returns an empty enumerable if items is null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? items)
    {
        return items ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Returns true if items include all types.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="includeAssignableTypes">If false only exact types are considered.</param>
    /// <param name="types">The types to be searched.</param>
    /// <returns></returns>
    public static bool ExistsType<T>(this IEnumerable<T> items, bool includeAssignableTypes, params Type[] types)
    {
        items = items.ThrowIfEnumerableIsNull();
        types.ThrowIfOutOfRange(() => types.Length == 0);

        var search = types.ToList();

        Func<Type, Type, Type?> checkType = includeAssignableTypes ? ofType : ofExactType;

        foreach (var item in items)
        {
            if (null == item) continue;

            var type = item.GetType();
            var checkedtype = search.Select(x => checkType(x, type)).FirstOrDefault(x => null != x);

            if (null != checkedtype)
            {
                search.Remove(checkedtype);

                if (0 == search.Count) return true;
            }
        }

        static Type? ofExactType(Type lhs, Type rhs)
        {
            return lhs.Equals(rhs) ? lhs : null;
        }

        static Type? ofType(Type lhs, Type rhs)
        {
            var exactType = ofExactType(lhs, rhs);
            if (null != exactType) return exactType;

            return lhs.IsAssignableFrom(rhs) ? lhs : null;
        }

        return false;
    }

    public static IElseIf<T> If<T>(
        this IEnumerable<T> items,
        Func<T, bool> predicate,
        Action<T> action)
    {
        predicate.ThrowIfNull();
        action.ThrowIfNull();

        var @else = Enumerable.Empty<T>();

        foreach (var item in items)
        {
            if (predicate(item))
            {
                action(item);
                continue;
            }
            @else = @else.Append(item);
        }
        return new ElseIf<T>(@else);
    }

    public static IElseIf<T, TResult> If<T, TResult>(
        this IEnumerable<T> items,
        Func<T, bool> predicate,
        Func<T, TResult> selector)
    {
        predicate.ThrowIfNull();
        selector.ThrowIfNull();

        var @else = new List<(T? lhs, TResult? rhs, bool isLhs)>();

        foreach (var item in items)
        {
            if (predicate(item))
            {
                var selected = selector(item);
                @else.Add((lhs: default(T?), rhs: selected, isLhs: false));
                continue;
            }
            @else.Add((lhs: item, rhs: default(TResult?), isLhs: true));
        }
        return new ElseIf<T, TResult>(@else, selector);
    }

    //public static IElse<T, TResult> If<T, TResult>(
    //    this IEnumerable<T> items,
    //    Func<T, bool> predicate,
    //    Func<T, TResult> map)
    //{
    //    predicate.ThrowIfEnumerableIsNull();
    //    map.ThrowIfEnumerableIsNull();

    //    return new ElseResult<T, TResult>(items, predicate, map);
    //}

    /// <summary>
    /// If items is empty <paramref name="whenEmpty"/> is called otherwise <paramref name="whenNotEmpty"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="whenNotEmpty"></param>
    /// <param name="whenEmpty"></param>
    /// <returns></returns>
    public static TResult IfAny<T, TResult>(
        this IEnumerable<T> items,
        Func<IEnumerable<T>, TResult> whenNotEmpty,
        Func<IEnumerable<T>, TResult> whenEmpty)
    {
        return items.Any() ? whenNotEmpty(items) : whenEmpty(items);
    }

    /// <summary>
    /// Returns alternative elements if enumerable is empty..
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs">if not empty this elements are the result</param>
    /// <param name="rhs">Alternative elements are the result if lhs is empty.</param>
    /// <returns></returns>
    public static IEnumerable<T> IfEmpty<T>(this IEnumerable<T> lhs, IEnumerable<T> rhs)
    {
        rhs.ThrowIfEnumerableIsNull();

        var lIt = lhs.GetEnumerator();
        if (!lIt.MoveNext())
        {
            foreach (var r in rhs)
            {
                yield return r;
            }
            yield break;
        }

        yield return lIt.Current;

        while (lIt.MoveNext())
        {
            yield return lIt.Current;
        }
    }

    /// <summary>
    /// If items is empty <paramref name="whenEmpty" /> otherwise items are returned.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items">List of items.</param>
    /// <param name="whenEmpty">Is called when items is empty.</param>
    /// <returns></returns>
    public static IEnumerable<T> IfEmpty<T>(this IEnumerable<T> items, Func<IEnumerable<T>> whenEmpty)
    {
        whenEmpty.ThrowIfNull();

        var it = items.GetEnumerator();
        if (!it.MoveNext())
        {
            foreach (var x in whenEmpty())
            {
                yield return x;
            }
            yield break;
        }

        yield return it.Current;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    /// <summary>
    /// If items is empty <paramref name="whenEmpty"/> is called otherwise <paramref name="whenNotEmpty"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="whenEmpty"></param>
    /// <param name="whenNotEmpty"></param>
    /// <returns></returns>
    public static TResult IfEmpty<T, TResult>(
        this IEnumerable<T> items,
        Func<IEnumerable<T>, TResult> whenEmpty,
        Func<IEnumerable<T>, TResult> whenNotEmpty)
    {
        return items.Any() ? whenNotEmpty(items) : whenEmpty(items);
    }

    public static IEnumerable<T> IfMoreOrEqualThan<T>(this IEnumerable<T> items, int numberOfItems)
    {
        var it = items.ThrowIfEnumerableIsNull().GetEnumerator();
        if (0 >= numberOfItems) yield break;

        var minimum = new List<T>();
        while (it.MoveNext())
        {
            minimum.Add(it.Current);
            if (minimum.Count == numberOfItems) break;
        }

        if (0 == minimum.Count || minimum.Count < numberOfItems) yield break;

        foreach (var item in minimum)
            yield return item;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }

    public static IEnumerable<T> IfMoreThan<T>(this IEnumerable<T> items, int numberOfItems)
    {
        var it = items.ThrowIfEnumerableIsNull().GetEnumerator();
        if (0 >= numberOfItems) yield break;

        var minimum = new List<T>();
        while(it.MoveNext())
        {
            minimum.Add(it.Current);
            if (minimum.Count > numberOfItems) break;
        }

        if (0 == minimum.Count) yield break;

        foreach (var item in minimum)
            yield return item;

        while (it.MoveNext())
        {
            yield return it.Current;
        }
    }


    /// <summary>
    /// Returns true if all items are in an ascending order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="allowEqual">if true equal values return true otherwise false.</param>
    /// <returns></returns>
    public static bool IsInAscendingOrder<T>(this IEnumerable<T> items, bool allowEqual = false)
        where T : IComparable<T>
    {
        items.ThrowIfEnumerableIsNull();

        var it = items.GetEnumerator();

        if (!it.MoveNext()) return true;

        var prev = it.Current;
        while (it.MoveNext())
        {
            var compare = prev.CompareTo(it.Current);

            var isAscending = allowEqual ? 1 > compare : -1 == compare;
            if (!isAscending) return false;

            prev = it.Current;
        }

        return true;
    }

    /// <summary>
    /// Returns true if all items are in an ascending order.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="compare"></param>
    /// <returns></returns>
    public static bool IsInAscendingOrder<T>(this IEnumerable<T> items, Func<T, T, CompareResult> compare)
    {
        items.ThrowIfEnumerableIsNull();
        compare.ThrowIfNull();

        var it = items.GetEnumerator();

        if (!it.MoveNext()) return true;

        var prev = it.Current;
        while (it.MoveNext())
        {
            if (CompareResult.Greater == compare(prev, it.Current))
                return false;

            prev = it.Current;
        }

        return true;
    }

    /// <summary>
    /// Returns true if items is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static bool IsNullOrEmpty<T>(this IEnumerable<T>? items)
    {
        if (items == null) return true;
        return !items.Any();
    }

    /// <summary>
    /// Checks if <paramref name="rhs"/> is a subset of <paramref name="lhs"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="lhs"></param>
    /// <param name="rhs"></param>
    /// <returns></returns>
    public static bool IsSubsetOf<T>(this IEnumerable<T> lhs, IEnumerable<T> rhs)
    {
        rhs.ThrowIfEnumerableIsNull();

        var search = new HashSet<T>(lhs);
        return search.IsSubsetOf(rhs);
    }

    /// <summary>
    /// Only calls <paramref name="selector"/> if <paramref name="modify"/> returns true otherwise returns items unmodified.
    /// </summary>
    /// <typeparam name="T">Type of item(s)</typeparam>
    /// <param name="items">List which should be modified.</param>
    /// <param name="modify">If true <paramref name="selector"/> is called.</param>
    /// <param name="selector">Is called when <paramref name="modify"/> returns true.</param>
    /// <returns></returns>
    public static IEnumerable<T> ModifyIf<T>(this IEnumerable<T> items, Func<bool> modify, Func<IEnumerable<T>, IEnumerable<T>> selector)
    {
        modify.ThrowIfNull();

        return modify() ? selector(items) : items;
    }

    /// <summary>
    /// Filters null items. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> NotNull<T>(this IEnumerable<T?> items)
    {
        if (null == items) yield break;

        foreach (var item in items)
        {
            if (null == item) continue;

            yield return item;
        }
    }

    /// <summary>
    /// Returns all items not of type <paramref name="types"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    public static IEnumerable<T> NotOfType<T>(this IEnumerable<T> items, params Type[] types)
    {
        return items.NotOfType(x => x, types);
    }

    /// <summary>
    /// Returns all items not of type <paramref name="types"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <param name="items"></param>
    /// <param name="selector"></param>
    /// <param name="types"></param>
    /// <returns></returns>
    public static IEnumerable<TResult> NotOfType<T, TResult>(
        this IEnumerable<T> items,
        Func<T, TResult> selector,
        params Type[] types)
    {
        foreach (var item in items.ThrowIfEnumerableIsNull())
        {
            if (null == item) continue;

            var itemType = item.GetType();

            if (types.Any(t => t.Equals(itemType) || t.IsAssignableFrom(itemType))) continue;

            yield return selector(item);
        }
    }

    /// <summary>
    /// Throws an ArgumentNullException if an element of the enumerable is null.
    /// Use this method only for value objects with small collections because the check is done in an eager way.
    /// Don't use <see cref="ThrowIfElementNull"/> for general collections because of performance reasons and also to keep the collection lazy. 
    /// Attention: This method runs into an endless loop when using with a generator!
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static IEnumerable<T> ThrowIfElementNull<T>(this IEnumerable<T> items)
    {
        return items.Any(x => null == x) ? throw new ArgumentNullException(nameof(items)) : items;
    }

    /// <summary>
    /// Throws an ArgumentNullException if the enumerable is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> ThrowIfEnumerableIsEmpty<T>(this IEnumerable<T> items, [CallerArgumentExpression("items")] string name = "")
    {
        return ThrowIfEnumerableIsEmpty(items, () => new ArgumentException("enumerable was empty", name));
    }

    /// <summary>
    /// Throws an Exception if items is empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="exceptionFactory"></param>
    /// <returns></returns>
    public static IEnumerable<T> ThrowIfEnumerableIsEmpty<T>(this IEnumerable<T> items, Func<Exception> exceptionFactory)
    {
        if (!items.Any())
        {
            var exception = exceptionFactory();
            throw exception;
        }
        return items;
    }

    /// <summary>
    /// Throws exception if items is null.
    /// </summary>
    /// <typeparam name="T">Type of element.</typeparam>
    /// <param name="items">Elements of the enumerable.</param>
    /// <param name="name">name of the enumerable.</param>
    /// <returns></returns>
    public static IEnumerable<T> ThrowIfEnumerableIsNull<T>(this IEnumerable<T> items, [CallerArgumentExpression("items")] string name = "")
    {
        return items.ThrowIfNull(name);
    }
            
    /// <summary>
    /// Throws an Exception if items is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="name"></param>
    /// <returns></returns>
    public static IEnumerable<T> ThrowIfEnumerableIsNullOrEmpty<T>(this IEnumerable<T> items, [CallerArgumentExpression("items")] string name = "")
    {
        return items.ThrowIfNull(name)
                    .ThrowIfEnumerableIsEmpty(name);
    }

    /// <summary>
    /// Throws an Exception if items is null or empty.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="exceptionFactory"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static IEnumerable<T> ThrowIfEnumerableIsNullOrEmpty<T>(this IEnumerable<T> items, Func<Exception> exceptionFactory)
    {
        exceptionFactory.ThrowIfNull();
        exceptionFactory.ThrowIfNull();

        return items.ThrowIfNull()
                    .ThrowIfEnumerableIsEmpty(exceptionFactory);
    }

    /// <summary>
    /// Returns <paramref name="numberOfElements"/> if items contains exactly <paramref name="numberOfElements"/> or throws an excption.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <param name="numberOfElements"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">is thrown when number of elements differs from  <paramref name="numberOfElements"/></exception>
    public static IEnumerable<T> ThrowIfNumberNotExact<T>(this IEnumerable<T> items, int numberOfElements)
    {
        if (!items.TakeExact(numberOfElements).Any())
            throw new ArgumentException($"items does not have exact {numberOfElements} elements");

        return items;
    }

    /// <summary>
    /// Returns an empty enumerable if items is null.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="items"></param>
    /// <returns></returns>
    public static IEnumerable<T> ToEmptyIfNull<T>(this IEnumerable<T>? items)
    {
        return items ?? Enumerable.Empty<T>();
    }
}

public interface IElseIf<T>
{
    IEnumerable<T> Else();
    IEnumerable<T> Else(Action<T> action);
    IElseIf<T> ElseIf(Func<T, bool> condition, Action<T> action);
    void EndIf();
}

public interface IElseIf<T, TResult>
{
    IEnumerable<TResult> Else();
    IEnumerable<TResult> Else(Func<T, TResult> selector);
    IElseIf<T, TResult> ElseIf(Func<T, bool> condition, Func<T, TResult> selector);
    void EndIf();
}


public interface IElse<T, TResult>
{
    IEnumerable<TResult> Else(Func<T, TResult> map);
}
