using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BSharp.Core;

/// <summary>
/// Custom <see cref="System.Diagnostics.Debug"/>-style stuff.
/// </summary>
public static class Bebug
{
    /// <inheritdoc cref="Debug.Assert(bool)"/>
    /// <remarks>
    /// This is identical to <see cref="Debug.Assert(bool)"/>, but uses the <see cref="CallerArgumentExpressionAttribute"/> for the message.
    /// </remarks>
    /// <param name="_condition">see <see cref="CallerArgumentExpressionAttribute"/></param>
    [Conditional("DEBUG")]
    public static void Assert(
        [DoesNotReturnIf(false)] bool condition,
        [CallerArgumentExpression(nameof(condition))]
        string _condition = ""
    )
    {
        Debug.Assert(condition, _condition);
    }
}