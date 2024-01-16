﻿namespace Foundation;

/// <summary>
/// This class can be used to avoid state.
/// </summary>
public static class Scope
{
    /// <summary>
    /// Returns a value from a scope. This can be used to avoid state.
    /// This enables e.g. return a value from an if or foreach statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static T Returns<T>(Func<T> ret)
    {
        ret.ThrowIfNull();
        return ret();
    }

    /// <summary>
    /// Returns a value asynchronous from a scope. This can be used to avoid state.
    /// This enables e.g. return a value from an if or foreach statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static async Task<T> ReturnsAsync<T>(Func<Task<T>> ret)
    {
        ret.ThrowIfNull();
        return await ret();
    }

    /// <summary>
    /// Returns a list of values from a scope. This can be used to avoid state. This enables e.g. return values from an if or foreach statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static IEnumerable<T> ReturnsMany<T>(Func<IEnumerable<T>> ret)
    {
        ret.ThrowIfNull();
        return ret();
    }

    /// <summary>
    /// Returns an nullable from a scope. This can be used to avoid state. This enables e.g. return a value from an if or foreach statement.
    /// </summary>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static T? ReturnsNullable<T>(Func<T?> ret)
    {
        ret.ThrowIfNull();
        return ret();
    }

    /// <summary>
    /// Returns an option from a scope. This can be used to avoid state. This enables e.g. return a value from an if or foreach statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static Option<T> ReturnsOption<T>(Func<Option<T>> ret)
    {
        ret.ThrowIfNull();
        return ret();
    }

    /// <summary>
    /// Returns an option from a scope. If decision true ret is executed and returned.
    /// This can be used to avoid state. This enables e.g. return a value from an if or foreach statement.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="decision">If true ret is executed and returned.</param>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static Option<T> ReturnsOption<T>(Func<bool> decision, Func<T> ret)
    {
        ret.ThrowIfNull();

        if (decision()) return Option.Maybe(ret());
        return Option.None<T>();
    }

    /// <summary>
    ///  Returns a <see cref="Result<typeparamref name="TOk"/>,<typeparamref name="TError"/>"/> from a scope.
    ///  This can be used to avoid state. This enables e.g. return a value from an if or foreach statement.
    /// </summary>
    /// <typeparam name="TOk"></typeparam>
    /// <typeparam name="TError"></typeparam>
    /// <param name="ret"></param>
    /// <returns></returns>
    public static Result<TOk, TError> ReturnsResult<TOk, TError>(Func<Result<TOk, TError>> ret)
    {
        ret.ThrowIfNull();
        return ret();
    }
}
