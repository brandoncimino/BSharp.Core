using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace BSharp.Core;

internal static class Hax
{
#if NET7_0_OR_GREATER
    /// <inheritdoc cref="CollectionsMarshal.AsSpan{T}"/>
#else
    /// <summary>
    /// Backport of <a href="https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.collectionsmarshal.asspan?view=net-8.0#system-runtime-interopservices-collectionsmarshal-asspan-1(system-collections-generic-list((-0)))">CollectionsMarshal.AsSpan&lt;T&gt;(List&lt;T&gt;)</a>.
    /// </summary>
#endif
    [Pure]
    public static Span<T> GetListSpan<T>(List<T>? list)
    {
#if NET7_0_OR_GREATER
        return CollectionsMarshal.AsSpan(list);
    }
#else
        // TODO: This is the gross, slow, reflection-based version.
        // Alternatively, I could use: https://github.com/atcarter714/UnityH4xx/tree/main
        // ...which even has a similar name to my stuff...!
        if (list == null)
        {
            return default;
        }

        var itemsField = list.GetType().GetField("_items",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        Bebug.Assert(itemsField != null);
        var itemsArray = (T[])itemsField.GetValue(list);
        Bebug.Assert(itemsArray != null);
        return itemsArray.AsSpan(0, list.Count);
    }
#endif

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{TElem}"/> representing <paramref name="stuff"/>, if possible.
    /// </summary>
    /// <param name="stuff">A sequence of <typeparamref name="TElem"/>s.</param>
    /// <param name="span">The <see cref="ReadOnlySpan{TElem}"/> that backs <paramref name="stuff"/>, if one exists.</param>
    /// <typeparam name="TStuff">An <see cref="IEnumerable{TElem}"/> type.</typeparam>
    /// <typeparam name="TElem">The type of the elements in <paramref name="stuff"/>.</typeparam>
    /// <returns><c>true</c> if <typeparamref name="TStuff"/> could be represented as a <see cref="ReadOnlySpan{T}"/></returns>
    /// <remarks>
    /// This has goofy generic arguments so that, ideally, it can avoid unnecessarily boxing <see cref="ValueType"/>s like <see cref="ImmutableArray{T}"/>
    /// when they get cast to <see cref="IEnumerable{T}"/>.
    ///
    /// TODO: Confirm that this ACTUALLY DOES avoid boxing of <see cref="ImmutableArray{T}"/>, while <see cref="TryGetReadOnlySpan{TElem}"/> does not.
    /// </remarks>
    public static bool TryGetReadOnlySpan<TStuff, TElem>([NoEnumeration] TStuff stuff, out ReadOnlySpan<TElem> span)
        where TStuff : IEnumerable<TElem>
    {
        switch (stuff)
        {
            case TElem[] array:
                span = array;
                return true;
            case List<TElem> list:
                span = GetListSpan(list);
                return true;
            case ImmutableArray<TElem> immer:
                span = immer.AsSpan();
                return true;
            case ArraySegment<TElem> segment:
                span = segment;
                return true;
            case string str:
                // WE know that `TElem` must be `char`, but the compiler doesn't, so we have to create a span ourselves.
                Bebug.Assert(typeof(TElem) == typeof(char), "How can a string be any other kind of enumerable?!");

#if NET6_0_OR_GREATER
                ref char startChar = ref Unsafe.AsRef(in str.GetPinnableReference());
#else
                ref char startChar = ref MemoryMarshal.GetReference<char>(str);
#endif
                ref TElem startElem = ref Unsafe.As<char, TElem>(ref startChar);
                span = MemoryMarshal.CreateReadOnlySpan(ref startElem, str.Length);
                return true;
            default:
                // TODO: Is this supposed to have some `[SkipLocalsInit]` nonsense?
                // TODO: Is there any meaningful difference between `default(ReadOnlySpan<T>)` and `ReadOnlySpan<T>.Empty`?
                span = default;
                return false;
        }
    }

    /// <summary>
    /// Gets a <see cref="ReadOnlySpan{T}"/> representation of the <paramref name="source"/>, if one exists.
    /// <p/>
    /// 📎 The most common types that can be represented as a <see cref="ReadOnlySpan{T}"/> are <see cref="Array"/>s, <see cref="List{T}"/>s, and <see cref="string"/>s.
    /// </summary>
    /// <param name="source">A collection that might be <see cref="ReadOnlySpan{T}"/>-friendly.</param>
    /// <param name="span">The <see cref="ReadOnlySpan{T}"/> representation of <paramref name="source"/>, if one exists.</param>
    /// <typeparam name="T">The sequence element type.</typeparam>
    /// <returns>true if <paramref name="source"/> could be represented as a <see cref="ReadOnlySpan{T}"/>.</returns>
    /// <remarks>
    /// The overload <see cref="TryGetReadOnlySpan{TStuff,TElem}"/> can occasionally be used to avoid boxing <see cref="source"/>.
    /// </remarks>
    public static bool TryGetReadOnlySpan<T>([NoEnumeration] IEnumerable<T> source, out ReadOnlySpan<T> span)
    {
        return TryGetReadOnlySpan<IEnumerable<T>, T>(source, out span);
    }
}