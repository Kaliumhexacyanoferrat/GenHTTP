namespace GenHTTP.Modules.Reflection.Operations;

public enum OperationResultSink
{
    Formatter,
    Serializer,
    Dynamic,
    Stream,
    None
}

public class OperationResult
{

    #region Get-/Setters

    public OperationResultSink Sink { get; }

    public Type Type { get; }

    #endregion

    #region Initialization

    public OperationResult(Type type, OperationResultSink sink)
    {
        Type = type;
        Sink = sink;
    }

    #endregion

}
