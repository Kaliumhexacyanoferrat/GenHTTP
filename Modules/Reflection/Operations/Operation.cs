using System.Reflection;

namespace GenHTTP.Modules.Reflection.Operations;

public sealed class Operation
{

    #region Get-/Setters

    public MethodInfo Method { get; }

    public OperationPath Path { get; }

    public IReadOnlyDictionary<string, OperationArgument> Arguments { get; }

    #endregion

    #region Initialization

    public Operation(MethodInfo method, OperationPath path, IReadOnlyDictionary<string, OperationArgument> arguments)
    {
        Method = method;
        Path = path;
        Arguments = arguments;
    }

    #endregion

}
