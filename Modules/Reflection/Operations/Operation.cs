namespace GenHTTP.Modules.Reflection.Operations;

public sealed class Operation
{

    #region Get-/Setters

    public OperationPath Path { get; }

    public IReadOnlyDictionary<string, OperationArgument> Arguments { get; }

    #endregion

    #region Initialization

    public Operation(OperationPath path, IReadOnlyDictionary<string, OperationArgument> arguments)
    {
        Path = path;
        Arguments = arguments;
    }

    #endregion

}
