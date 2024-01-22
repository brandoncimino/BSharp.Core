namespace BSharp.Core;

public static class RandomExtensions
{
    private static readonly Random SharedRandom =
#if NET7_0_OR_GREATER
        Random.Shared;
#else
    new Random();
#endif
    
    /// <returns>this <see cref="Random"/>, if it is non-<c>null</c>; otherwise, <see cref="Random.Shared"/></returns>
    [Pure]
    public static Random OrShared(this Random? random) => random ?? SharedRandom;
    
    /// <param name="generator">a <see cref="Random"/> instance <i>(defaults to <see cref="Random.Shared"/>)</i></param>
    /// <param name="stuff">some stuff to choose from</param>
    /// <typeparam name="T">the type of <paramref name="stuff"/></typeparam>
    /// <returns>a random <see cref="IList{T}.this"/> element from <paramref name="stuff"/></returns>
    [Pure]
    public static T NextElement<T>(this Random generator, IList<T> stuff) => stuff[generator.Next(stuff.Count)];

    /// <inheritdoc cref="NextElement{T}(System.Random,System.Collections.Generic.IList{T})"/>
    [Pure]
    public static T NextElement<T>(this Random generator, ReadOnlySpan<T> stuff) =>
        stuff[generator.Next(stuff.Length)];

    /// <inheritdoc cref="NextElement{T}(System.Random,System.Collections.Generic.IList{T})"/>
    [Pure]
    public static T NextElement<T>(this Random generator, Span<T> stuff) => stuff[generator.Next(stuff.Length)];
    
    /// <param name="stuff">some stuff to choose from</param>
    /// <param name="generator">a <see cref="Random"/> instance <i>(defaults to <see cref="Random.Shared"/>)</i></param>
    /// <typeparam name="T">the type of <paramref name="stuff"/></typeparam>
    /// <returns>a random <see cref="IList{T}.this"/> element from <paramref name="stuff"/></returns>
    [Pure]
    public static T RandomElement<T>(this IList<T> stuff, Random? generator = null) =>
        generator.OrShared().NextElement(stuff);

    /// <inheritdoc cref="RandomElement{T}(System.Collections.Generic.IList{T},System.Random?)"/>
    /// <returns>a random <see cref="ReadOnlySpan{T}.this"/> element from <paramref name="stuff"/></returns>
    [Pure]
    public static T RandomElement<T>(this ReadOnlySpan<T> stuff, Random? generator = null) =>
        generator.OrShared().NextElement(stuff);

    /// <inheritdoc cref="RandomElement{T}(System.Collections.Generic.IList{T},System.Random?)"/>
    /// <returns>a random <see cref="Span{T}.this"/> element from <paramref name="stuff"/></returns>
    [Pure]
    public static T RandomElement<T>(this Span<T> stuff, Random? generator = null) =>
        generator.OrShared().NextElement(stuff);
}