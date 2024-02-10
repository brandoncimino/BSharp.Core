using System.Runtime.CompilerServices;
using System.Text;

namespace BSharp.Core;

public static class EnumerableExtensions
{
    #region JoinString

    /// <inheritdoc cref="string.Concat(System.Collections.Generic.IEnumerable{string?})"/>
    /// <remarks>
    /// This is an extension method version of <see cref="M:System.String.Concat``1(System.Collections.Generic.IEnumerable{``0})"/>.
    /// </remarks>
    public static string JoinString<T>(this IEnumerable<T> values) => string.Concat(values);

    /// <inheritdoc cref="M:System.String.Join``1(System.Char,System.Collections.Generic.IEnumerable{``0})"/>
    /// <remarks>
    /// This is an extension method version of <see cref="M:System.String.Join``1(System.Char,System.Collections.Generic.IEnumerable{``0})"/>.
    /// </remarks>
    public static string JoinString<T>(this IEnumerable<T> values, char separator) => string.Join(separator, values);

    /// <summary>
    /// Joins together the <see cref="string"/> representations of <paramref name="values"/>, with <paramref name="separator"/> in between each entry,
    /// and bookended by <paramref name="prefix"/> and <paramref name="suffix"/>.
    /// </summary>
    /// <param name="values">The stuff that you want to join together into a single <see cref="string"/>.</param>
    /// <param name="separator">A string inserted between each of the <paramref name="values"/>.</param>
    /// <param name="prefix">An optional string at the beginning of the result.</param>
    /// <param name="suffix">An optional string at the end of the result.</param>
    /// <typeparam name="T">The source element type.</typeparam>
    /// <returns>A new <see cref="string"/>.</returns>
    public static string JoinString<T>(
        this IEnumerable<T> values,
        ReadOnlySpan<char> separator,
        ReadOnlySpan<char> prefix = default,
        ReadOnlySpan<char> suffix = default
    )
    {
        // This is based on `String.JoinCore`: https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs,796
        if (typeof(T) == typeof(string))
        {
            var stringValues = Unsafe.As<IEnumerable<T>, IEnumerable<string?>>(ref values);
            if (Hax.TryGetReadOnlySpan(stringValues, out var valueSpan))
            {
                return JoinString_AllStrings(valueSpan, separator, prefix, suffix);
            }
        }


#if NET6_0_OR_GREATER
        return JoinString_InterpolatedHandler<IEnumerable<T>, T>(values, separator, prefix, suffix);
#else
        return JoinString_StringBuilder(values, separator, prefix, suffix);
#endif
    }

    [MethodImpl(MethodImplOptions
        .AggressiveInlining) /* This is generally used in loops, so hopefully inlining it will help optimize those loops */]
    private static void Write(this Span<char> buffer, ReadOnlySpan<char> toWrite, ref int pos)
    {
        toWrite.CopyTo(buffer[pos..]);
        pos += toWrite.Length;
    }

    [Pure]
    private static string JoinString_AllStrings(
        ReadOnlySpan<string?> strings,
        ReadOnlySpan<char> separator,
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> suffix)
    {
        return strings switch
        {
            [] => ConcatStrings(prefix, suffix),
            [var a] => ConcatStrings(prefix, a, suffix),
            _ => JoinString_AllStrings_TwoOrMore(strings, separator, prefix, suffix)
        };
    }

    [Pure]
    private static string JoinString_AllStrings_TwoOrMore(
        ReadOnlySpan<string?> strings,
        ReadOnlySpan<char> separator,
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> suffix
    )
    {
        Bebug.Assert(strings.Length >= 2);

        var lengthFromStrings = GetTotalLength(strings);

        var lengthFromSeparators = prefix.Length + suffix.Length + (strings.Length - 1) * separator.Length;
        var finalLength = lengthFromStrings + lengthFromSeparators;
        Span<char> buffer = stackalloc char[finalLength];

        int pos = 0;
        buffer.Write(prefix, ref pos);

        // TODO: Benchmark to see if separating out index [0] and then looping from 1 onward is actually faster than having an `if` statement for `i == 0` inside of the loop
        buffer.Write(strings[0], ref pos);
        for (int i = 1; i < buffer.Length; i++)
        {
            buffer.Write(prefix, ref pos);
            buffer.Write(strings[i], ref pos);
        }

        buffer.Write(suffix, ref pos);
        Bebug.Assert(pos == buffer.Length,
            $"We should have filled the entire buffer ({buffer.Length}), but only wrote {pos} characters!");
        return pos.ToString();
    }

    /// TODO: This can probably be optimized.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int GetTotalLength(ReadOnlySpan<string?> strings)
    {
        var lengthFromStrings = 0;
        foreach (var s in strings)
        {
            lengthFromStrings += s?.Length ?? 0;
        }

        return lengthFromStrings;
    }

    /// <summary>
    /// I have no idea how this number was originally derived, but I'm taking it from <a href="https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs,39">F:System.String.StackallocCharBufferSizeLimit</a>.
    /// </summary>
    private const int StackallocCharBufferSizeLimit = 256;

#if NET6_0_OR_GREATER
    /// <summary>
    /// Joins together stuff into a big <see cref="string"/> via <see cref="DefaultInterpolatedStringHandler"/>.
    ///
    /// TODO: Benchmark this to make sure that it's <i>actually</i> faster than just using a <see cref="StringBuilder"/>.
    /// </summary>
    /// <remarks>
    /// This is based on the <see cref="ISpanFormattable"/> branch of <a href="https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs,854">String.JoinCore</a>.
    /// <p/>
    /// In <a href="https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs,854">String.JoinCore</a>, <see cref="DefaultInterpolatedStringHandler"/> is only used when <typeparamref name="T"/> is an <see cref="ISpanFormattable"/> <see cref="ValueType"/>.
    /// If it isn't, they use to <a href="https://source.dot.net/System.Private.CoreLib/src/libraries/Common/src/System/Text/ValueStringBuilder.cs.html">ValueStringBuilder</a> instead (<a href="https://source.dot.net/#System.Private.CoreLib/src/libraries/System.Private.CoreLib/src/System/String.Manipulation.cs,885">String.Manipulation.cs, line 885</a>).
    /// <br/>
    /// However, since we don't have access to <c>ValueStringBuilder</c>, and since <see cref="DefaultInterpolatedStringHandler"/> works for non-<see cref="ISpanFormattable"/>s anyways, we just always use it. 
    /// </remarks>
    private static string JoinString_InterpolatedHandler<TEnumerable, T>(
        TEnumerable stuff,
        ReadOnlySpan<char> separator,
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> suffix
    ) where TEnumerable : IEnumerable<T>
    {
        // If know the number of items, we know how many `separator`s we'll need.
        // In this case, we can more efficiently instantiate the `DefaultInterpolatedStringHandler`.
        if (stuff.TryGetNonEnumeratedCount(out var count))
        {
            if (count == 0)
            {
                return string.Concat(prefix, suffix);
            }

            // 📎 This doesn't actually have to correspond to usages of `.AppendLiteral()` - it's just used to help the system
            //   guess at how big a buffer to rent while it starts building the string.
            var literalLength = prefix.Length + suffix.Length + (separator.Length * (count - 1));
            return AppendJoin(
                new DefaultInterpolatedStringHandler(literalLength, count,
                    System.Globalization.CultureInfo.CurrentCulture),
                stuff,
                separator,
                prefix,
                suffix
            );
        }

        return AppendJoin(
            // ⚠ `literalLength` and `formattedCount` are COMPLETELY IGNORED by this constructor!
            new DefaultInterpolatedStringHandler(
                0,
                0,
                System.Globalization.CultureInfo.CurrentCulture,
                stackalloc char[StackallocCharBufferSizeLimit]
            ),
            stuff,
            separator,
            prefix,
            suffix
        );

        static string AppendJoin(
            DefaultInterpolatedStringHandler handler,
            TEnumerable stuff,
            ReadOnlySpan<char> separator,
            ReadOnlySpan<char> prefix,
            ReadOnlySpan<char> suffix
        )
        {
            using var erator = stuff.GetEnumerator();
            if (!erator.MoveNext())
            {
                return string.Concat(prefix, suffix);
            }

            handler.AppendFormatted(prefix);
            handler.AppendFormatted(erator.Current);

            while (erator.MoveNext())
            {
                handler.AppendFormatted(separator);
                handler.AppendFormatted(erator.Current);
            }

            handler.AppendFormatted(suffix);
            return handler.ToStringAndClear();
        }
    }
#else
    /// <summary>
    /// Joins stuff together using a plain-old <see cref="StringBuilder"/>.
    /// This should only be used when fancier methods, like interpolated string handlers, aren't available.
    /// </summary>
    private static string JoinString_StringBuilder<T>(
        IEnumerable<T> source,
        ReadOnlySpan<char> separator,
        ReadOnlySpan<char> prefix,
        ReadOnlySpan<char> suffix)
    {
        using var erator = source.GetEnumerator();
        if (!erator.MoveNext())
        {
            return ConcatStrings(prefix, suffix);
        }

        var sb = new StringBuilder();
        sb.Append(prefix);
        sb.Append(erator.Current);
        while (erator.MoveNext())
        {
            sb.Append(separator);
            sb.Append(erator.Current);
        }

        sb.Append(suffix);
        return sb.ToString();
    }
#endif

    /// Backwards-compatible version of <see cref="string.Concat(ReadOnlySpan{char}, ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining) /*I dunno, seems like something a cool kid would do*/]
    private static string ConcatStrings(ReadOnlySpan<char> first, ReadOnlySpan<char> second)
    {
#if NET5_0_OR_GREATER
        return string.Concat(first, second);
#else
        var len = first.Length + second.Length;
        if (len == 0)
        {
            return "";
        }

        Span<char> span = stackalloc char[len];
        first.CopyTo(span);
        second.CopyTo(span[first.Length..]);
        return span.ToString();
#endif
    }

    /// Backwards-compatible version of <see cref="string.Concat(ReadOnlySpan{char}, ReadOnlySpan{char}, ReadOnlySpan{char})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining) /*I dunno, seems like something a cool kid would do*/]
    private static string ConcatStrings(ReadOnlySpan<char> first, ReadOnlySpan<char> second, ReadOnlySpan<char> third)
    {
#if NET5_0_OR_GREATER
        return string.Concat(first, second, third);
#else
        if (first.Length + second.Length + third.Length == 0)
        {
            return "";
        }

        Span<char> span = stackalloc char[first.Length + second.Length];
        first.CopyTo(span);
        second.CopyTo(span[first.Length..]);
        return span.ToString();
#endif
    }

    #endregion
}