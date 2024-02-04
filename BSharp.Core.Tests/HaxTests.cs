using System.Collections.Immutable;
using System.Reflection;
using NUnit.Framework;

namespace BSharp.Core.Tests;

public class HaxTests
{
    private static T[] GetListBackingArray_ViaReflection<T>(List<T> list)
    {
        var field = list.GetType().GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
        var got = field?.GetValue(list) as T[];
        return got ?? throw new ApplicationException($"Unable to retrieve the `_items` array from the list {list}!");
    }

    private static Span<T> GetExpectedListSpan<T>(List<T> list)
    {
        var span = GetListBackingArray_ViaReflection(list).AsSpan(0, list.Count);

#if NET6_0_OR_GREATER
        // An extra check in higher .NET versions to make sure that our reflection-based approach is returning the same thing as CollectionsMarshal
        var realSpan = System.Runtime.InteropServices.CollectionsMarshal.AsSpan(list);
        System.Diagnostics.Debug.Assert(span == realSpan);
#endif
        return span;
    }

    // #if NET8_0_OR_GREATER

    [Test]
    public void GetListSpan([Values(0, 1, 20, 21, 22, 23, 24, 25, 99, 101)] int listCount)
    {
        // We want to add each item 1 at a time to increase the chances that the list is backed by an array of a different size (i.e. ls.Count != ls.Capacity)
        var ls = new List<int>();
        for (int i = 0; i < listCount; i++)
        {
            ls.Add(i);
        }

        var span = Hax.GetListSpan(ls);
        var realSpan = GetExpectedListSpan(ls);
        Assert.That(span == realSpan);
    }

    private delegate ReadOnlySpan<TElements> AsRoSpanFunc<in TStuff, TElements>(TStuff stuff);

    [Test]
    public void TryGetReadOnlySpan()
    {
        Assert.Multiple(() =>
        {
            TryGetReadOnlySpan("", it => it.AsSpan());
            TryGetReadOnlySpan("abc", it => it.AsSpan());
            TryGetReadOnlySpan(ImmutableArray<int>.Empty, it => it.AsSpan());
            TryGetReadOnlySpan<int[], int>([1, 2, 3], it => it);
            TryGetReadOnlySpan<int[], int>([], it => it);
            TryGetReadOnlySpan<List<int>, int>([1, 2, 3], it => GetExpectedListSpan(it));
            TryGetReadOnlySpan<List<int>, int>(new List<int>(), it => GetExpectedListSpan(it));
            TryGetReadOnlySpan<ArraySegment<int>, int>(ArraySegment<int>.Empty, it => it);
            TryGetReadOnlySpan<ArraySegment<int>, int>(new ArraySegment<int>([1, 2, 3], 1, 2), it => it);
            TryGetReadOnlySpan<ArraySegment<int>, int>(new ArraySegment<int>([1, 2, 3], 1, 0), it => it);
        });
    }

    private static void TryGetReadOnlySpan<TStuff, TElem>(TStuff stuff, AsRoSpanFunc<TStuff, TElem> roSpanFunc)
        where TStuff : IEnumerable<TElem>
    {
        Assert.That(Hax.TryGetReadOnlySpan(stuff, out var actual)
                    && actual == roSpanFunc(stuff), Is.True, $"Span from: [{typeof(TStuff)}] {stuff}");
    }
}