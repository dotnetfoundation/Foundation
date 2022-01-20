﻿namespace Foundation;

using Foundation.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

public static class StringExtensions
{
    public static int CompareRegardingNumbers(this string? lhs, string? rhs, bool ignoreCase = false)
    {
        if (string.IsNullOrEmpty(lhs)) return string.IsNullOrEmpty(rhs) ? 0 : -1;
        if (string.IsNullOrEmpty(rhs)) return 1;

        var lhsIsNumber = double.TryParse(lhs, out double lhsNumber);
        var rhsIsNumber = double.TryParse(rhs, out double rhsNumber);
        if (lhsIsNumber)
        {
            if (rhsIsNumber) return lhsNumber.CompareTo(rhsNumber);
            return -1;
        }
        else if (rhsIsNumber)
        {
            return 1;
        }

        return string.Compare(lhs, rhs, ignoreCase);
    }

    public static IEnumerable<int> IndexesFromEnd(this string str, char value, int stopAfterNumberOfHits = -1)
    {
        var index = str.Length - 1;
        var numberOfHits = 0;

        while (0 <= index)
        {
            if (-1 < stopAfterNumberOfHits && numberOfHits >= stopAfterNumberOfHits) break;

            if (str[index] == value)
            {
                numberOfHits++;
                yield return index;
            }
            index--;
        }
    }

    public static IEnumerable<int> IndexesFromEnd(this string str, string value, int stopAfterNumberOfHits = -1)
    {
        var index = str.Length - 1;
        var numberOfHits = 0;

        while (0 <= index)
        {
            if (-1 < stopAfterNumberOfHits && numberOfHits >= stopAfterNumberOfHits) break;

            var startIndex = index - value.Length;
            if (0 > startIndex) break;

            var sub = str[startIndex..index];
            if (value == sub)
            {
                numberOfHits++;
                yield return startIndex;
            }
            index--;
        }
    }

    public static IEnumerable<int> IndexesOf(this string str, [DisallowNull] string value, int stopAfterNumberOfHits = -1)
    {
        var numberOfHits = 0;
        var index = str.IndexOf(value);

        while (-1 != index)
        {
            numberOfHits++;
            if (-1 < stopAfterNumberOfHits && numberOfHits > stopAfterNumberOfHits) break;

            yield return index;
            if (str.Length < (index + 1)) break;
            index = str.IndexOf(value, index + 1);
        }
    }

    public static IEnumerable<int> IndexesOfAny(this string str, char[] values, int stopAfterNumberOfHits = -1)
    {
        var numberOfHits = 0;
        var i = 0;
        foreach (var c in str)
        {
            if (-1 < stopAfterNumberOfHits && numberOfHits > stopAfterNumberOfHits) break;

            if (values.Contains(c))
            {
                numberOfHits++;
                yield return i;
            }

            i++;
        }
    }

    public static IEnumerable<int> IndexesOfAny(this string str, string[] values, int stopAfterNumberOfHits = -1)
    {
        var numberOfHits = 0;
        var index = values.Min(v => str.IndexOf(v));

        while (-1 != index)
        {
            numberOfHits++;
            if (-1 < stopAfterNumberOfHits && numberOfHits > stopAfterNumberOfHits) break;

            yield return index;
            if (str.Length < (index + 1)) break;

            var indices = values.Select(v => str.IndexOf(v, index + 1)).Where(i => -1 != i);
            index = indices.Any() ? indices.Min() : -1;
        }
    }

    public static int IndexFromEnd(this string str, char value) => IndexFromEnd(str, str.Length - 1, value);

    public static int IndexFromEnd(this string str, string value) => IndexFromEnd(str, str.Length, value);

    public static int IndexFromEnd(this string str, int index, char value)
    {
        while (0 <= index)
        {
            if (str[index] == value) return index;

            index--;
        }
        return -1;
    }

    public static int IndexFromEnd(this string str, int index, string value)
    {
        while (0 <= index)
        {
            var startIndex = index - value.Length;
            if (0 > startIndex) break;

            var sub = str[startIndex..index];
            if (value == sub) return startIndex;
            index--;
        }
        return -1;
    }

    /// <summary>
    /// starts searching from the end.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="values"></param>
    /// <returns></returns>
    public static int IndexOfAnyFromEnd(this string str, char[] values) => IndexOfAnyFromEnd(str, str.Length - 1, values);
    
    public static int IndexOfAnyFromEnd(this string str, int index, char[] values)
    {
        while (0 <= index)
        {
            var c = str[index];
            if (values.Contains(c)) return index;
            index--;
        }
        return -1;
    }

    public static bool IsNumeric(this string str)
    {
        if (decimal.TryParse(str, out decimal _))
            return true;

        return false;
    }

    public static Opt<object> ParseScalarType(this string str, [DisallowNull] Type type)
    {
        type.ThrowIfNull(nameof(type));

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => bool.TryParse(str, out bool boolean)
                   ? Opt.Some<object>(boolean)
                   : Opt.None<object>(),
            TypeCode.Byte => byte.TryParse(str, out byte @byte)
                   ? Opt.Some<object>(@byte)
                   : Opt.None<object>(),
            TypeCode.Char => char.TryParse(str, out char @char)
                   ? Opt.Some<object>(@char)
                   : Opt.None<object>(),
            TypeCode.DateTime => DateTimeHelper.TryParseFromIso8601(str, out DateTime dt)
                   ? Opt.Some<object>(dt)
                   : Opt.None<object>(),
            TypeCode.Decimal => decimal.TryParse(str, out decimal @decimal)
                   ? Opt.Some<object>(@decimal)
                   : Opt.None<object>(),
            TypeCode.Double => double.TryParse(str, out double @double)
                   ? Opt.Some<object>(@double)
                   : Opt.None<object>(),
            TypeCode.Int16 => Int16.TryParse(str, out Int16 @int16)
                   ? Opt.Some<object>(@int16)
                   : Opt.None<object>(),
            TypeCode.Int32 => Int32.TryParse(str, out Int32 @int32)
                   ? Opt.Some<object>(@int32)
                   : Opt.None<object>(),
            TypeCode.Int64 => Int64.TryParse(str, out Int64 @int64)
                   ? Opt.Some<object>(@int64)
                   : Opt.None<object>(),
            TypeCode.SByte => SByte.TryParse(str, out SByte @sbyte)
                   ? Opt.Some<object>(@sbyte)
                   : Opt.None<object>(),
            TypeCode.Single => Single.TryParse(str, out Single @single)
                   ? Opt.Some<object>(@single)
                   : Opt.None<object>(),
            TypeCode.String => Opt.Some<object>(str),
            TypeCode.UInt16 => UInt16.TryParse(str, out UInt16 @uint16)
                   ? Opt.Some<object>(@uint16)
                   : Opt.None<object>(),
            TypeCode.UInt32 => UInt32.TryParse(str, out UInt32 @uint32)
                   ? Opt.Some<object>(@uint32)
                   : Opt.None<object>(),
            TypeCode.UInt64 => UInt64.TryParse(str, out UInt64 @uint64)
                   ? Opt.Some<object>(@uint64)
                   : Opt.None<object>(),
            _ => Opt.None<object>()
        };
    }

    /// <summary>
    /// Multiple spaces in sequence will be reduced to one space.
    /// </summary>
    /// <param name="str"></param>
    /// <returns></returns>
    public static string ReduceSpaces(this string str)
    {
        var re = new Regex("  +");
        var matches = re.Matches(str);
        if (0 == matches.Count) return str;

        var sb = new StringBuilder();
        var nextPos = 0;
        foreach (Match m in matches)
        {
            var substr = str.Substring(nextPos, m.Index - nextPos + 1);
            sb.Append(substr);
            nextPos = m.Index + m.Length;
        }

        var end = str[nextPos..];
        if (!string.IsNullOrWhiteSpace(end))
            sb.Append(end);

        return sb.ToString();
    }

    public static IEnumerable<string> SplitAtIndex(this string str, params int[] indexes)
    {
        if (0 == indexes.Length)
        {
            yield return str;
            yield break;
        }

        var i = 0;
        var maxIndex = str.Length - 1;
        var orderedIndexes = indexes.OrderBy(n => n);

        foreach (var index in orderedIndexes)
        {
            if (0 >= index) continue;

            if (i > maxIndex || index > maxIndex)
                yield break;

            yield return str.SubstringFromIndex(i, index - 1);
            i = index;
        }

        if (i < str.Length)
            yield return str.SubstringFromIndex(i, str.Length - 1);
    }

    /// <summary>
    /// creates a list of separators with strings between separators.
    /// 
    /// example:
    /// string: "A123B456C789"
    /// result: {'A', "123"}, {'B', "456"}, {'C', "789"}
    /// 
    /// </summary>
    /// <param name="str"></param>
    /// <param name="separator"></param>
    /// <returns></returns>
    public static IEnumerable<KeyValuePair<char, string>> SplitSeq(this string str, char[] separator)
    {
        var key = Opt.None<char>();
        var sb = new StringBuilder();
        var len = str.Length;
        var i = 0;
        foreach (var c in str)
        {
            if (separator.Contains(c))
            {
                if (key.IsSome)
                {
                    yield return Pair.New(key.Value, sb.ToString());
                    sb.Clear();
                }
                key = Opt.Some(c);
                i++;
                if (i == len)
                    yield return Pair.New(key.Value, sb.ToString());

                continue;
            }
            if (key.IsSome)
                sb.Append(c);

            i++;
        }
    }

    /// <summary>
    /// searches a substring between to strings.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="inclusive">if true left and right are included.</param>
    /// <returns></returns>
    public static string? SubstringBetween(this string str, string left, string right, bool inclusive = true)
    {
        var indexes = IndexesOfAny(str, new[] { left, right }).ToArray();
        if (2 != indexes.Length) return null;

        var leftIndex = indexes[0];
        if (-1 == leftIndex) return null;

        var rightIndex = indexes[1];
        if (-1 == rightIndex) return null;

        rightIndex += right.Length - 1;
        if (!inclusive)
        {
            leftIndex += left.Length;
            if (leftIndex == rightIndex) return null;

            rightIndex -= right.Length;
            if (leftIndex > rightIndex) return null;
        }

        return SubstringFromIndex(str, leftIndex, rightIndex);
    }

    /// <summary>
    /// returns a substring between two strings. The search starts from the end.
    /// </summary>
    /// <param name="str"></param>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="inclusive"></param>
    /// <returns></returns>
    public static string? SubstringBetweenFromEnd(this string str, string left, string right, bool inclusive = true)
    {
        var rightIndex = IndexFromEnd(str, right);
        if (-1 == rightIndex) return null;

        var leftIndex = IndexFromEnd(str, rightIndex - 1, left);
        if (-1 == leftIndex) return null;

        return inclusive 
            ? str[leftIndex..(rightIndex + right.Length)] 
            : str[(leftIndex + left.Length)..rightIndex];
    }


    public static string SubstringFromIndex(this string str, int start, int end)
    {
        if (null == str) throw new ArgumentNullException(nameof(str));
        if (start > end) throw new ArgumentOutOfRangeException(nameof(start), $"must not be greater than {nameof(end)}");
        if (str.Length < (start - 1)) throw new IndexOutOfRangeException(nameof(start));
        if (str.Length < (end - 1)) throw new IndexOutOfRangeException(nameof(end));

        return str.Substring(start, end - start + 1);
    }
    
    [return: NotNull]
    public static string ThrowIfNullOrEmpty(this string str, [DisallowNull] string argumentName)
    {
        if (string.IsNullOrEmpty(str)) throw new ArgumentNullException(argumentName);
        return str;
    }

    [return: NotNull]
    public static string ThrowIfNullOrWhiteSpace(this string str, [DisallowNull] string argumentName)
    {
        if (string.IsNullOrWhiteSpace(str)) throw new ArgumentNullException(argumentName);
        return str;
    }

    public static double? ToDoubleAsNullable(this string str)
    {
        if (double.TryParse(str, out double value))
            return value;

        return null;
    }

    public static Opt<double> ToDoubleAsOpt(this string str)
    {
        if (double.TryParse(str, out double value))
            return Opt.Some(value);

        return Opt.None<double>();
    }

    public static T? ToNullableIfNullOrEmpty<T>(this string str, [DisallowNull] Func<string, T> projection)
       where T : struct
    {
        projection.ThrowIfNull(nameof(projection));

        if (string.IsNullOrEmpty(str)) return null;

        return projection(str);
    }

    public static T? ToNullableIfNullOrWhiteSpace<T>(this string str, [DisallowNull] Func<string, T> projection)
        where T : struct
    {
        projection.ThrowIfNull(nameof(projection));

        if (string.IsNullOrWhiteSpace(str)) return null;

        return projection(str);
    }

    public static object? ToNumber(this string str)
    {
        if (double.TryParse(str, NumberStyles.Number, CultureInfo.InvariantCulture, out double doubleValue))
            return doubleValue;

        return null;
    }

    public static object? ToNumber(this string str, NumberStyles style, IFormatProvider provider)
    {
        provider.ThrowIfNull(nameof(provider));

        if (double.TryParse(str, style, provider, out double doubleValue))
            return doubleValue;

        if (decimal.TryParse(str, style, provider, out decimal decimalValue))
            return decimalValue;

        return null;
    }

    public static Opt<string> ToOptIfNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str) ? Opt.None<string>() : Opt.Some(str);
    }

    public static Opt<T> ToOptIfNullOrEmpty<T>(this string str, [DisallowNull] Func<string, T> projection)
    {
        projection.ThrowIfNull(nameof(projection));

        return string.IsNullOrEmpty(str) ? Opt.None<T>() : Opt.Some(projection(str));
    }

    public static Opt<string> ToOptIfNullOrWhiteSpace(this string str)
    {
        return string.IsNullOrWhiteSpace(str) ? Opt.None<string>() : Opt.Some(str);
    }

    public static Opt<T> ToOptIfNullOrWhiteSpace<T>(this string str, [DisallowNull] Func<string, T> projection)
    {
        projection.ThrowIfNull(nameof(projection));

        return string.IsNullOrWhiteSpace(str) ? Opt.None<T>() : Opt.Some(projection(str));
    }

    public static string TrimApostrophes(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;

        var apostrophes = new[] { '\'', '\"' };
        var start = 0;
        var length = str.Length;
        if (apostrophes.Any(c => c == str[0]))
        {
            start = 1;
            length--;
        }

        if (apostrophes.Any(c => c == str[str.Length - 1]))
            length--;

        return str.Substring(start, length);
    }
}

