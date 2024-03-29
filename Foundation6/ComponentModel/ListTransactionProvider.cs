// The MIT License (MIT)
//
// Copyright (c) 2020 Markus Raufer
//
// All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
﻿using System.Collections;

namespace Foundation.ComponentModel;

public static class ListTransactionProvider
{
    /// <summary>
    /// Creates a new <see cref="ListTransactionProvider{T}"/>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <returns></returns>
    public static ListTransactionProvider<T> New<T>(IList<T> list) => new(list);
}

/// <summary>
/// This decorates a list as <see cref="ITransactionProvider{TTransaction}"/>
/// </summary>
/// <typeparam name="T">The type of the list elements.</typeparam>
public class ListTransactionProvider<T> 
    : IList<T>
    , ITransactionProvider<ITransaction<Guid>>
{
    private IDisposable? _commited;
    private readonly List<ActionEvent> _events = new();
    private readonly IList<T> _list;

    #region events
    private record ActionEvent(ListAction Action);
    private record IndexEvent(ListAction Action, int Index) : ActionEvent(Action);
    private record IndexValueEvent(ListAction Action, int Index, T Value) : ValueEvent(Action, Value);
    private record ValueEvent(ListAction Action, T Value) : ActionEvent(Action);
    #endregion events

    public ListTransactionProvider(IList<T> list)
    {
        _list = list.ThrowIfNull();
    }

    public T this[int index]
    { 
        get => _list[index];
        set => _events.Add(new IndexValueEvent(ListAction.Replace, index, value)); 
    }

    public ITransaction<Guid> BeginTransaction()
    {
        _events.Clear();

        var transaction = new Transaction();
        _commited = transaction.Committed.Subscribe(OnTransactionCommitted);

        return transaction;
    }

    public int Count => _list.Count;

    public bool IsReadOnly => _list.IsReadOnly;

    public void Add(T item) => _events.Add(new ValueEvent(ListAction.Add, item));

    public void Clear() => _events.Add(new ActionEvent(ListAction.Clear));

    public bool Contains(T item) => _list.Contains(item);

    public void CopyTo(T[] array, int arrayIndex) => _list.CopyTo(array, arrayIndex);

    public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

    public int IndexOf(T item) => _list.IndexOf(item);

    public void Insert(int index, T item) => _events.Add(new IndexValueEvent(ListAction.Insert, index, item));

    private void OnTransactionCommitted()
    {
        foreach(var @event in _events)
        {
            switch(@event)
            {
                case ValueEvent e when e.Action == ListAction.Add: _list.Add(e.Value); break;
                case ActionEvent e when e.Action == ListAction.Clear: _list.Clear(); break;
                case IndexValueEvent e when e.Action == ListAction.Insert: _list.Insert(e.Index, e.Value); break;
                case ValueEvent e when e.Action == ListAction.Remove: _list.Remove(e.Value); break;
                case IndexEvent e when e.Action == ListAction.RemoveAt: _list.RemoveAt(e.Index); break;
                case IndexValueEvent e when e.Action == ListAction.Replace: _list[e.Index] = e.Value; break;
            }
        }

        _commited?.Dispose();
    }

    public bool Remove(T item)
    {
        if (_list.Contains(item))
        {
            _events.Add(new ValueEvent(ListAction.Remove, item));
            return true;
        }

        return false;
    }

    public void RemoveAt(int index) => _events.Add(new IndexEvent(ListAction.RemoveAt, index));

    IEnumerator IEnumerable.GetEnumerator() => _list.GetEnumerator();
}
