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
            argumentTypes.Add(operation.Result.Type);
        }

        var stringTypes = string.Join(", ", argumentTypes.Select(CompilationUtil.GetQualifiedName));

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

        var typeName = CompilationUtil.GetQualifiedName(operation.Method.DeclaringType!);

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

        sb.Append(isVoid ? "        " : "        var result = ");

        if (isAsync)
        {
            sb.Append("await ");
        }

        sb.AppendLine($"{invoker}(");
        sb.AppendArgumentList(operation);
        sb.AppendLine("        );");
    }

    private static void AppendArgumentList(this StringBuilder sb, Operation operation)
    {
        var i = 0;

        foreach (var argument in operation.Arguments)
        {
            // todo: for reference types the default is also null (e.g. for strings this might should be string.Empty)
            sb.Append($"            arg{i + 1} ?? default");

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
