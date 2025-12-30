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
        
        return typeof(IResultWrapper).IsAssignableFrom(returnType);
    }

}
