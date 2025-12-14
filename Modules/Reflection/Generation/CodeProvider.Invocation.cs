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
        var arguments = string.Join(", ", operation.Arguments.Select(x => x.Key));
        
        var type = (operation.Result.Sink == OperationResultSink.None) ? "Action" : "Func";
            
        var argumentTypes = new List<Type>(operation.Arguments.Select(x => x.Value.Type));

        if (operation.Result.Sink != OperationResultSink.None)
        {
            argumentTypes.Add(operation.Result.Type);
        }

        var stringTypes = string.Join(", ", argumentTypes);
            
        sb.AppendLine($"        var typedLogic = ({type}<{stringTypes}>)logic;");
        sb.AppendLine();
            
        sb.AppendLine($"        var result = typedLogic({arguments});");
    }

    public static void AppendMethodInvocation(this StringBuilder sb, Operation operation)
    {
        var arguments = string.Join(", ", operation.Arguments.Select(x => x.Key));

        var methodName = operation.Method.Name;

        var typeName = operation.Method.DeclaringType!.Name;

        sb.AppendLine($"        var typedInstance = ({typeName})instance;");
        sb.AppendLine();
        sb.AppendLine($"        var result = typedInstance.{methodName}({arguments});");
    }
    
}
