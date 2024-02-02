#if !NET7_0_OR_GREATER
// ReSharper disable CheckNamespace
namespace System.Linq;

internal static partial class Enumerable
{
    public static bool TryGetNonEnumeratedCount<T>(this IEnumerable<T> source, out int count)
    {
        if (source is IList<T> list)
        {
            count = list.Count;
            return true;
        }

        count = default;
        return false;
    }
}
#endif