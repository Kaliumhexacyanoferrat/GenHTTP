using System.Reflection;

namespace GenHTTP.Modules.Reflection.Operations;

public sealed class Operation
{

    #region Initialization

    public Operation(MethodInfo method, OperationPath path, OperationResult result, IReadOnlyDictionary<string, OperationArgument> arguments)
    {
        Method = method;
        Path = path;
        Result = result;
        Arguments = arguments;
    }

    #endregion

    #region Get-/Setters

    public MethodInfo Method { get; }

    public OperationPath Path { get; }

    public IReadOnlyDictionary<string, OperationArgument> Arguments { get; }

    public OperationResult Result { get; }

    #endregion

}
