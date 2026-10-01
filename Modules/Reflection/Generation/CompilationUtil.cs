using GenHTTP.Api.Protocol;
using GenHTTP.Modules.Reflection.Operations;

using Microsoft.CodeAnalysis.CSharp;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CompilationUtil
{
    private static readonly Dictionary<Type, string> BuiltInTypes = new()
    {
        { typeof(int), "int" },
        { typeof(string), "string" },
        { typeof(bool), "bool" },
        { typeof(void), "void" },
        { typeof(object), "object" },
        { typeof(long), "long" },
        { typeof(short), "short" },
        { typeof(byte), "byte" },
        { typeof(double), "double" },
        { typeof(float), "float" },
        { typeof(decimal), "decimal" },
        { typeof(char), "char" }
    };

    internal static string GetSafeString(ByteString name)
        => GetSafeString(name.ToString());
    
    internal static string GetSafeString(string input)
        => SyntaxFactory.Literal(input).ToFullString();
    
    internal static string GetQualifiedName(Type type, bool allowNullable)
    {
        if (BuiltInTypes.TryGetValue(type, out var keyword))
            return keyword;

        if (Nullable.GetUnderlyingType(type) is Type underlyingType)
            return $"{GetQualifiedName(underlyingType, false)}" + (allowNullable ? "?" : string.Empty);

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();

            var name = genericType.FullName!;
            name = name[..name.IndexOf('`')].Replace('+', '.');

            return $"{name}<{string.Join(", ", args.Select(a => GetQualifiedName(a, allowNullable)))}>";
        }

        if (type.IsArray)
            return $"{GetQualifiedName(type.GetElementType()!, allowNullable)}[]";

        return type.FullName!.Replace('+', '.');
    }
    
    internal static bool CanHoldNull(Type type)
    {
        if (type == typeof(void))
            return false;

        if (type.IsAsyncVoid())
            return false;

        if (type.IsGenericallyVoid())
            return false;
        
        if (!type.IsValueType)
            return true;
        
        return Nullable.GetUnderlyingType(type) != null;
    }

    internal static bool HasWrappedResult(Operation operation)
    {
        var returnType = operation.Method.ReturnType;

        if (returnType.IsAsyncGeneric())
        {
            returnType = returnType.GenericTypeArguments[0];
        }

        return typeof(IResultWrapper).IsAssignableFrom(returnType);
    }

    /// <summary>
    /// Renders the given constant as a C# expression.
    /// </summary>
    /// <param name="value">The constant to be rendered</param>
    /// <returns>The C# expression or null, if the constant cannot be expressed as a literal</returns>
    internal static string? GetLiteral(object value) => value switch
    {
        string s => GetSafeString(s),
        bool b => b ? "true" : "false",
        int i => SyntaxFactory.Literal(i).ToFullString(),
        uint ui => SyntaxFactory.Literal(ui).ToFullString(),
        long l => SyntaxFactory.Literal(l).ToFullString(),
        ulong ul => SyntaxFactory.Literal(ul).ToFullString(),
        char c => SyntaxFactory.Literal(c).ToFullString(),
        decimal m => SyntaxFactory.Literal(m).ToFullString(),
        double d => double.IsFinite(d) ? SyntaxFactory.Literal(d).ToFullString() : GetNonFiniteLiteral("double", d),
        float f => float.IsFinite(f) ? SyntaxFactory.Literal(f).ToFullString() : GetNonFiniteLiteral("float", f),
        byte or sbyte or short or ushort => $"(({GetQualifiedName(value.GetType(), false)}){Convert.ToInt32(value)})",
        Enum e => $"(({GetQualifiedName(e.GetType(), false)})({GetLiteral(Convert.ChangeType(e, Enum.GetUnderlyingType(e.GetType())))}))",
        _ => null
    };

    private static string GetNonFiniteLiteral(string type, double value)
    {
        if (double.IsNaN(value))
        {
            return $"{type}.NaN";
        }

        return (value > 0) ? $"{type}.PositiveInfinity" : $"{type}.NegativeInfinity";
    }

}
