using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using NUnit.Framework;

namespace BSharp.Core.Tests;

public class Backport_CallerArgumentExpressionAttributeTests
{
    [Test]
    public void CallerArgumentExpressionWorks()
    {
        var actual = CaptureExpression(DayOfWeek.Monday.Equals(DateTime.Today.DayOfWeek));
        Assert.That(actual, Is.EqualTo("DayOfWeek.Monday.Equals(DateTime.Today.DayOfWeek)"));
    }

    private static string? CaptureExpression<T>(
        [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Global")]
        [SuppressMessage("ReSharper", "EntityNameCapturedOnly.Local")]
        T expression,
        [CallerArgumentExpression(nameof(expression))]
        string? _expression = null)
    {
        return _expression;
    }
}