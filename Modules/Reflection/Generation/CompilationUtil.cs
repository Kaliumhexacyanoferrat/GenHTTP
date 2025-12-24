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

    public static string GetSafeString(string input)
        => SyntaxFactory.Literal(input).ToFullString();
    
    public static string GetQualifiedName(Type type)
    {
        if (BuiltInTypes.TryGetValue(type, out var keyword))
            return keyword;

        if (type.IsGenericType)
        {
            var genericType = type.GetGenericTypeDefinition();
            var args = type.GetGenericArguments();

            var name = genericType.FullName!;
            name = name[..name.IndexOf('`')].Replace('+', '.');

            return $"{name}<{string.Join(", ", args.Select(GetQualifiedName))}>";
        }

        if (type.IsArray)
            return $"{GetQualifiedName(type.GetElementType()!)}[]";

        return type.FullName!.Replace('+', '.');
    }

}
