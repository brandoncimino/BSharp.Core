// ReSharper disable all

namespace System.Runtime.CompilerServices;

#if !NETCOREAPP3_1_OR_GREATER
[AttributeUsage(AttributeTargets.Parameter, Inherited = default, AllowMultiple = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    public string ParameterName { get; }
}
#endif