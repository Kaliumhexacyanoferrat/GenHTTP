namespace GenHTTP.Modules.Reflection;

public enum OperationArgumentSource
{
    Path,
    Query,
    Body
}

public sealed class OperationArgument
{

    #region Get-/Setters

    public string Name { get; }

    public OperationArgumentSource Source { get; }

    #endregion

    #region Initialization

    public OperationArgument(string name, OperationArgumentSource source)
    {
        Name = name;
        Source = source;
    }

    #endregion

}
