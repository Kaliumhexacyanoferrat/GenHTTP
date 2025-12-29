using System.Text;
using GenHTTP.Modules.Reflection.Operations;

namespace GenHTTP.Modules.Reflection.Generation;

public static class CodeProviderInvocationExtensions
{

    public static void AppendInvocation(this StringBuilder sb, Operation operation)
    {
        if (operation.Delegate != null)
        {
            sb.AppendDelegateInvocation(operation);
        }
        else
        {
            sb.AppendMethodInvocation(operation);
        }

        sb.AppendLine();
    }

    public static void AppendDelegateInvocation(this StringBuilder sb, Operation operation)
    {
        var type = (operation.Result.Sink == OperationResultSink.None) ? "Action" : "Func";

        var argumentTypes = new List<Type>(operation.Arguments.Select(x => x.Value.Type));

        if (operation.Result.Sink != OperationResultSink.None)
        {
            argumentTypes.Add(operation.Method.ReturnType);
        }

        var stringTypes = string.Join(", ", argumentTypes.Select(a => CompilationUtil.GetQualifiedName(a, true)));

        if (stringTypes.Any())
        {
            sb.AppendLine($"        var typedLogic = ({type}<{stringTypes}>)logic;");
        }
        else
        {
            sb.AppendLine($"        var typedLogic = ({type})logic;");
        }
        
        sb.AppendLine();

        sb.AppendInvocation(operation, "typedLogic");
    }

    public static void AppendMethodInvocation(this StringBuilder sb, Operation operation)
    {
        var methodName = operation.Method.Name;

        var typeName = CompilationUtil.GetQualifiedName(operation.Method.DeclaringType!, false);

        sb.AppendLine($"        var typedInstance = ({typeName})instance;");
        sb.AppendLine();

        sb.AppendInvocation(operation, $"typedInstance.{methodName}");
    }

    private static void AppendInvocation(this StringBuilder sb, Operation operation, string invoker)
    {
        var resultType = operation.Method.ReturnType;

        var isAsyncGeneric = resultType.IsAsyncGeneric();

        var isVoid = (isAsyncGeneric) ? resultType.IsGenericallyVoid() : resultType.IsAsyncVoid() || resultType == typeof(void);

        var isAsync = resultType.IsAsync();

        var wrapped = CompilationUtil.HasWrappedResult(operation);

        if (isVoid)
        {
            sb.Append("        ");
        }
        else
        {
            if (wrapped)
            {
                sb.Append("        var wrapped = ");
            }
            else
            {
                sb.Append("        var result = ");
            }
        }


        if (isAsync)
        {
            sb.Append("await ");
        }

        sb.AppendLine($"{invoker}(");
        sb.AppendArgumentList(operation);
        sb.AppendLine("        );");

        if (wrapped)
        {
            sb.AppendLine();
            sb.AppendLine($"        var result = wrapped.Payload;");
        }
    }

    private static void AppendArgumentList(this StringBuilder sb, Operation operation)
    {
        var i = 0;

        foreach (var argument in operation.Arguments)
        {
            sb.Append($"            arg{i + 1}");
            
            var defaultIsNull = !argument.Value.Type.IsValueType || Nullable.GetUnderlyingType(argument.Value.Type) != null;
            
            if (!defaultIsNull)
            {
                sb.Append(" ?? default");
            }

            var last = (i++ == operation.Arguments.Count - 1);

            if (last)
            {
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine(",");
            }
        }
    }

}
