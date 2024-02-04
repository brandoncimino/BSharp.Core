// ReSharper disable all

namespace System.Runtime.CompilerServices;

#if !NET6_0_OR_GREATER
[AttributeUsage(AttributeTargets.Parameter, Inherited = default, AllowMultiple = false)]
public sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
#endif