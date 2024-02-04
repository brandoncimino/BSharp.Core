using NUnit.Framework;

namespace BSharp.Core.Tests;

public class JoinStringTests
{
    public static IEnumerable<int> Counts => Enumerable.Range(0, 10);
    public static IEnumerable<string> Strings => ["", null, "->", " "];

    [Test]
    public void JoinString_Simple([ValueSource(nameof(Counts))] int count, [Values] bool enumerateLazily)
    {
        var stuff = count.RandomStuff();
        var joined = stuff.MakeLazy(enumerateLazily).JoinString();
        var concat = string.Concat(stuff.MakeLazy(enumerateLazily));
        Assert.That(joined, Is.EqualTo(concat));
    }

    [Test]
    public void JoinString_CharJoiner([ValueSource(nameof(Counts))] int count, [Values] bool enumerateLazily)
    {
        var stuff = count.RandomStuff();
        var joined = stuff.MakeLazy(enumerateLazily).JoinString('-');
        var expected = string.Join('-', stuff.MakeLazy(enumerateLazily));
        Assert.That(joined, Is.EqualTo(expected));
    }

    [Test]
    public void JoinString_Complex(
        [ValueSource(nameof(Counts))] int count,
        [ValueSource(nameof(Strings))] string separator,
        [ValueSource(nameof(Strings))] string prefix,
        [ValueSource(nameof(Strings))] string suffix,
        [Values] bool enumerateLazily
    )
    {
        var stuff = count.RandomStuff();
        var joined = stuff.MakeLazy(enumerateLazily).JoinString(separator, prefix, suffix);
        var expected = prefix + string.Join(separator, stuff.MakeLazy(enumerateLazily)) + suffix;
        Assert.That(joined, Is.EqualTo(expected), $"Joined:\n\t{string.Join("\n\t", stuff)}");
    }
}