using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace BSharp.Core.Tests;

public static class TestData
{
    public static Random CreateRandom([CallerMemberName] string caller = null!)
    {
        if (caller == null)
        {
            throw new ArgumentNullException(nameof(caller), "how?!");
        }

        return new Random(caller.Sum(static c => c));
    }

    public static readonly ImmutableArray<object> Junk = ImmutableArray.Create<object>(
        1,
        Math.PI,
        "butts",
        MathF.E,
        new[] { "yolo", "swag" },
        Guid.NewGuid(),
        new DateTime(2019, 5, 15, 19, 3, 41)
    );

    public static object[] RandomStuff(this int size, [CallerMemberName] string caller = null!) =>
        RandomStuff(size, CreateRandom(caller), Junk);

    public static T[] RandomStuff<T>(int size, Random random, IList<T> stuff) =>
        Enumerable.Range(0, size)
            .Select(it => stuff.RandomElement(random))
            .ToArray();

    public static IEnumerable<T> MakeLazy<T>(this IEnumerable<T> stuff, bool shouldMakeLazy)
    {
        static IEnumerable<X> _MakeLazy<X>(IEnumerable<X> stuff)
        {
            foreach (var it in stuff)
            {
                yield return it;
            }
        }

        return shouldMakeLazy switch
        {
            true => _MakeLazy(stuff),
            false => stuff
        };
    }
}