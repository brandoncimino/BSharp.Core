using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace BSharp.Core.Tests;

public class RandomExtensionsTests
{
    private static Random CreateRandom([CallerMemberName] string? caller = null)
    {
        if (caller == null)
        {
            throw new ArgumentNullException(nameof(caller), "how?!");
        }

        return new Random(caller.Sum(static c => c));
    }

    [Test]
    public void NextElement_IList()
    {
        var choices = new[] { 1, 2, 3 };
        var viaExtension = CreateRandom().NextElement(choices);
        var viaVanilla = choices[CreateRandom().Next(choices.Length)];

        Assert.That(viaExtension, Is.EqualTo(viaVanilla));
    }

    [Test]
    public void NextElement_Span()
    {
        Span<int> choices = new[] { 1, 2, 3 };
        var viaExtension = CreateRandom().NextElement(choices);
        var viaVanilla = choices[CreateRandom().Next(choices.Length)];

        Assert.That(viaExtension, Is.EqualTo(viaVanilla));
    }

    [Test]
    public void NextElement_RoSpan()
    {
        ReadOnlySpan<int> choices = new[] { 1, 2, 3 };
        var viaExtension = CreateRandom().NextElement(choices);
        var viaVanilla = choices[CreateRandom().Next(choices.Length)];

        Assert.That(viaExtension, Is.EqualTo(viaVanilla));
    }
}