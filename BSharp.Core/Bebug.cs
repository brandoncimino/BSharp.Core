using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace BSharp.Core;

/// <summary>
/// Custom <see cref="System.Diagnostics.Debug"/>-style stuff.
/// </summary>
public static class Bebug
{
    [Conditional("DEBUG")]
    public static void Assert(
        bool condition,
        [CallerArgumentExpression(nameof(condition))]
        string _condition = ""
    )
    {
        Debug.Assert(condition, _condition);
    }
}