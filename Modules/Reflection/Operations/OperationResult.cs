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

    #region Initialization

    public OperationResult(Type type, OperationResultSink sink)
    {
        Type = type;
        Sink = sink;
    }

    #endregion

    #region Get-/Setters

    public OperationResultSink Sink { get; }

    public Type Type { get; }

    #endregion

}
